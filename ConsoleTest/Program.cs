// <copyright file="Program.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace UnderDeskBike
{
    /// <summary>
    /// This program is for testing the Under Desk Bike from the command line.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The bike data log.
        /// </summary>
        private static readonly StreamWriter FileLog = new("datalog.txt", append: true);

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "No parameters")]
        public static async Task Main(string[] args)
        {
            await Run().ConfigureAwait(false);
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        private static async Task Run()
        {
            FileLog.WriteLine($"Starting run on {DateTime.Now}");
            using var bike = new Bike(null);
            bike.Connected += Bike_Connected;
            bike.Disconnected += Bike_Disconnected;
            bike.WorkoutUpdate += Bike_WorkoutUpdate;

            while (!bike.IsConnected)
            {
                Console.WriteLine("Waiting for bike...");
                await bike.WaitForConnection();
                Console.WriteLine("Connected.");
                await Task.Delay(1000);
            }

            Console.WriteLine("Starting workout...");
            await bike.StartWorkout();
            Console.WriteLine("Press a key to quit");
            while (!Console.KeyAvailable)
            {
                await Task.Yield();
            }
            Console.ReadKey(true);
            await bike.StopWorkout();

            FileLog.WriteLine();
            FileLog.WriteLine();
            FileLog.Close();
        }

        /// <summary>
        /// Handles the WorkoutUpdate event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BikeWorkoutEventArgs"/> instance containing the event data.</param>
        private static void Bike_WorkoutUpdate(object sender, BikeWorkoutEventArgs e)
        {
            Console.Write($"{e.Data.DistanceKms:n2} kms".PadLeft(10));
            Console.Write($"{e.Data.RotationsPerMinute:n0} RPM".PadLeft(10));
            Console.Write($"{e.Data.SpeedKph:n2} kph".PadLeft(10));
            Console.Write($"Speed: {e.Data.SpeedValue}".PadLeft(10));
            Console.Write(e.Data.WorkoutTime.ToString("g").PadLeft(10));
            Console.WriteLine();
        }

        /// <summary>
        /// Handles the Disconnected event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void Bike_Disconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Bike Disconnected.");
        }

        /// <summary>
        /// Handles the Connected event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void Bike_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("Bike Connected.");
        }
    }
}

// <copyright file="App.xaml.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

using Codaris.Common;

using Microsoft.Win32;

namespace UnderDeskBike
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Gets the current app instance.
        /// </summary>
        public static new App Current => (App)Application.Current;

        /// <summary> The name of the application.</summary>
        public const string Name = "UnderDeskBike";

        /// <summary>
        /// Gets the settings tracker.
        /// </summary>
        public Jot.Tracker Settings { get; } = new Jot.Tracker();

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            Settings.Configure<Window>()
                .Id(w => $"{w.Name}, {SystemParameters.VirtualScreenWidth}, {SystemParameters.VirtualScreenHeight}")
                .Properties(w => new { w.Left, w.Top })
                .PersistOn(nameof(Window.Closed))
                .StopTrackingOn(nameof(Window.Closed));
        }

        /// <summary>
        /// Sets whether the app runs at startup or not.
        /// </summary>
        /// <param name="value">if set to <c>true</c> run at startup.</param>
        public static void SetRunAtStartup(bool value)
        {
            using var startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assert.IsNotNull(startupKey);
            if (!value)
            {
                if (startupKey.GetValue(Name) == null) return;
                startupKey.DeleteValue(Name);
                return;
            }
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null) return; 
            startupKey.SetValue(Name, mainModule.FileName);
        }

        /// <summary>
        /// Gets whether the app is running on startup.
        /// </summary>
        /// <returns>Returns <c>true</c> if the application is set to run at startup.</returns>
        public static bool GetRunAtStartup()
        {
            using var startupKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (startupKey == null) return false;
            var mainModule = Process.GetCurrentProcess().MainModule;
            if (mainModule == null) return false;   
            return (string?)startupKey.GetValue(Name) == mainModule.FileName;
        }
    }
}

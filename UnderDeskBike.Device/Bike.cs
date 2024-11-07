// <copyright file="Bike.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Codaris.Common;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace UnderDeskBike
{
    /// <summary>
    /// The primary class for the under desk bike.
    /// </summary>
    /// <seealso cref="Codaris.Common.NotifyObject" />
    /// <seealso cref="System.IDisposable" />
    public class Bike : NotifyObject, IDisposable
    {
        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        public BikeWorkoutData WorkoutData { get => _workoutData; private set => SetProperty(ref _workoutData, value); }
        private BikeWorkoutData _workoutData = default;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        public bool IsConnected => bikeDevice.IsConnected;

        /// <summary>
        /// Gets a value indicating whether this instance is workout active.
        /// </summary>
        public bool IsWorkoutRunning => workoutCancellationTokenSource != null;

        /// <summary>
        /// Occurs when connected.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when disconnected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// Occurs when workout update occurs.
        /// </summary>
        public event EventHandler<BikeWorkoutEventArgs> WorkoutUpdate;

        /// <summary>
        /// Occurs when start workout.
        /// </summary>
        public event EventHandler WorkoutStarted;

        /// <summary>
        /// Occurs when ending workout.
        /// </summary>
        public event EventHandler WorkoutEnded;

        /// <summary>
        /// Occurs when a background error is triggered
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Error;

        /// <summary>
        /// The bike device.
        /// </summary>
        private BikeDevice bikeDevice;

        /// <summary>
        /// The cancellation token source for terminating the feed.
        /// </summary>
        private CancellationTokenSource workoutCancellationTokenSource = null;

        /// <summary>
        /// The run task completion source.
        /// </summary>
        private TaskCompletionSource workoutTaskCompletionSource = null;

        /// <summary>
        /// The connect task completion source.
        /// </summary>
        private TaskCompletionSource connectedTaskCompletionSource = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bike"/> class.
        /// </summary>
        /// <param name="errorHandler">The error handler.</param>
        /// <param name="logger">The logger.</param>
        public Bike(TextWriter logger = null)
        {
            bikeDevice = new BikeDevice(logger);
            bikeDevice.Connected += BikeDevice_Connected;
            bikeDevice.Disconnected += BikeDevice_Disconnected;
        }

        /// <summary>
        /// Start listening for a connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartListening()
        {
            await bikeDevice.StartListening();
        }

        /// <summary>
        /// Stops the listening.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopListening()
        {
            await StopWorkout();
            bikeDevice.StopListening();
        }

        /// <summary>
        /// Waits for connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task WaitForConnection()
        {
            if (bikeDevice.IsConnected) return;
            connectedTaskCompletionSource ??= new TaskCompletionSource();
            await bikeDevice.StartListening();
            await connectedTaskCompletionSource.Task;
        }

        /// <summary>
        /// Starts the workout.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<bool> StartWorkout(int interval = 250)
        {
            if (!IsConnected) return false;
            if (!await bikeDevice.ExecuteCommand(new BikeConnectCommand())) return false;
            Task.Run(() => RunWorkout(interval)).FireAndForget(ex =>
            {
                var exceptionEventArgs = new ExceptionEventArgs(ex);
                Error?.Invoke(this, exceptionEventArgs);
                return exceptionEventArgs.IsHandled;
            });
            return true;
        }

        /// <summary>
        /// Stops the workout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopWorkout()
        {
            if (!IsWorkoutRunning) return;
            workoutCancellationTokenSource.Cancel();
            if (workoutTaskCompletionSource == null) return;
            await workoutTaskCompletionSource.Task;
        }

        /// <summary>
        /// Waits for workout end.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task WaitForWorkout()
        {
            if (workoutCancellationTokenSource == null) return Task.CompletedTask;
            return workoutTaskCompletionSource.Task;
        }

        /// <summary>
        /// Handles the Disconnected event of the BikeDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BikeDevice_Disconnected(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsConnected));
            connectedTaskCompletionSource?.TrySetCanceled();
            connectedTaskCompletionSource = null;
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the Connected event of the BikeDevice control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BikeDevice_Connected(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(IsConnected));
            connectedTaskCompletionSource?.TrySetResult();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Starts the workout loop.
        /// </summary>
        /// <param name="interval">The interval.</param>
        /// <exception cref="InvalidOperationException">The workout has already been started.</exception>
        private async Task RunWorkout(int interval)
        {
            if (workoutCancellationTokenSource != null)
            {
                throw new InvalidOperationException("The workout has already been started.");
            }

            var started = false;

            try
            {
                // Create the cancellation token source
                workoutCancellationTokenSource = new CancellationTokenSource();
                workoutTaskCompletionSource = new TaskCompletionSource();
                var token = workoutCancellationTokenSource.Token;
                var errorCount = 0;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await bikeDevice.ExecuteCommand(new BikeWorkoutCommand(!started));
                        WorkoutData = result;
                        if (!started)
                        {
                            OnPropertyChanged(nameof(IsWorkoutRunning));
                            WorkoutStarted?.Invoke(this, EventArgs.Empty);
                        }
                        started = true;
                        errorCount = 0;
                        WorkoutUpdate?.Invoke(this, new BikeWorkoutEventArgs(result));
                    }
                    catch (BikeDataException) when (started)
                    {
                        // Ignore data exception error if started
                        errorCount++;
                        if (errorCount == 3) throw;
                    }
                    await Task.Delay(interval);
                }
            }
            finally
            {
                workoutCancellationTokenSource.Dispose();
                workoutCancellationTokenSource = null;
                workoutTaskCompletionSource.TrySetResult();
                workoutTaskCompletionSource = null;
                OnPropertyChanged(nameof(IsWorkoutRunning));
                WorkoutEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The disposed value.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    bikeDevice?.Dispose();
                    bikeDevice = null;
                }
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
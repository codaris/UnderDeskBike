// <copyright file="MainViewModel.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

using Codaris.Common;
using Codaris.Wpf;

using Microsoft.EntityFrameworkCore;

using UnderDeskBike.Models;

namespace UnderDeskBike.ViewModels
{
    /// <summary>
    /// The view model for the main window.
    /// </summary>
    /// <seealso cref="Codaris.Wpf.BaseViewModel" />
    public class MainViewModel : BaseViewModel
    {
        /// <summary>
        /// Gets the pause command.
        /// </summary>
        public ICommand StartCommand { get; } = null;

        /// <summary>
        /// Gets the stop command.
        /// </summary>
        public ICommand StopCommand { get; } = null;

        /// <summary>
        /// Gets the rotations per minute.
        /// </summary>
        public int RotationsPerMinute { get => _rotationsPerMinute; private set => SetProperty(ref _rotationsPerMinute, value); }
        private int _rotationsPerMinute;

        /// <summary>
        /// Gets the distance in KMs.
        /// </summary>
        public decimal DistanceKms { get => _distanceKms; private set => SetProperty(ref _distanceKms, value); }
        private decimal _distanceKms;

        /// <summary>
        /// Gets the speed in KPH.
        /// </summary>
        public decimal SpeedKph { get => _speedKph; private set => SetProperty(ref _speedKph, value); }
        private decimal _speedKph;

        /// <summary>
        /// Gets the workout time.
        /// </summary>
        public TimeSpan WorkoutTime { get => _workoutTime; private set => SetProperty(ref _workoutTime, value); }
        private TimeSpan _workoutTime;

        /// <summary>
        /// Gets a value indicating whether this instance is paused.
        /// </summary>
        public bool IsPaused { get => _isPaused; private set => SetProperty(ref _isPaused, value); }
        private bool _isPaused;

        /// <summary>
        /// Gets the status text.
        /// </summary>
        public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }
        private string _statusText = "Waiting for connection...";

        /// <summary>
        /// Gets the data text.
        /// </summary>
        public string SummaryText
        {
            get
            {
                if (bike.IsWorkoutRunning) return $"Today: {todayDistanceKms + bike.WorkoutData.DistanceKms:n1} kms";
                else return $"Today: {todayDistanceKms:n1} kms";
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether window is always on top.
        /// </summary>
        public bool AlwaysOnTop { get => _alwaysOnTop; set { SetProperty(ref _alwaysOnTop, value); ShowInTaskbar = !value; } }
        private bool _alwaysOnTop = true;

        /// <summary>
        /// Gets or sets a value indicating whether to run at application at startup.
        /// </summary>
        public bool RunAtStartup { get => _runAtStartup; set => SetProperty(ref _runAtStartup, value, App.SetRunAtStartup); }
        private bool _runAtStartup;

        /// <summary>
        /// Gets or sets a value indicating whether to show in taskbar.
        /// </summary>
        public bool ShowInTaskbar { get => _showInTaskbar; set => SetProperty(ref _showInTaskbar, !AlwaysOnTop || value); }
        private bool _showInTaskbar = false;

        /// <summary>The bike device.</summary>
        private readonly Bike bike = new Bike();

        /// <summary>
        /// The current workout.
        /// </summary>
        private Workout currentWorkout = null;

        /// <summary>
        /// The workout database context.
        /// </summary>
        private Context context = null;

        /// <summary>
        /// The paused timer.
        /// </summary>
        private readonly Timer pausedTimer = new Timer(30_000);

        /// <summary>The daily timer to reset the stats.</summary>
        private readonly ScheduledTimer dailyTimer = new ScheduledTimer(0, 0, 0);

        /// <summary>
        /// The today distance KMS.
        /// </summary>
        private decimal todayDistanceKms = 0;

        /// <summary>
        /// Occurs when error is triggered.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> Error;

        /// <summary>
        /// Occurs when status changed and the window should update
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            context = new Context();
            context.Database.Migrate();
            pausedTimer.Elapsed += PausedTimer_Elapsed;
            StartCommand = new RelayCommand(() => bike.StartWorkout(500).FireAndForget(RaiseErrorEvent), () => !bike.IsWorkoutRunning);
            StopCommand = new RelayCommand(() => bike.StopWorkout().FireAndForget(RaiseErrorEvent), () => bike.IsWorkoutRunning);
            bike.Connected += Bike_Connected;
            bike.Disconnected += Bike_Disconnected;
            bike.WorkoutUpdate += Bike_WorkoutUpdate;
            bike.WorkoutStarted += Bike_WorkoutStarted;
            bike.WorkoutEnded += Bike_WorkoutEnded;
            bike.Error += Bike_Error;

            // Setup the timer to run daily and reset the stats.
            // Call the event once directly to setup stats now.
            dailyTimer.AutoReset = true;
            dailyTimer.Elapsed += this.DailyTimer_Elapsed;
            DailyTimer_Elapsed(this, null);
            dailyTimer.Start();

            // Gets current run at startup value
            _runAtStartup = App.GetRunAtStartup();
        }

        /// <summary>
        /// Called when window loaded.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnWindowLoaded()
        {
            await bike.StartListening();
        }

        /// <summary>
        /// Called when window closed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task OnWindowClosed()
        {
            await bike.StopWorkout();
            await bike.WaitForWorkout();
            bike.Dispose();
            pausedTimer.Dispose();
            dailyTimer.Dispose();
        }

        /// <summary>
        /// Handles the Connected event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void Bike_Connected(object sender, EventArgs e)
        {
            StatusChanged?.Invoke(this, EventArgs.Empty);
            StatusText = "Starting Workout...";
            await bike.StartWorkout(500);
            RunOnUIThread(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// Handles the Disconnected event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Bike_Disconnected(object sender, EventArgs e)
        {
            StatusText = "Waiting for connection...";
            RunOnUIThread(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// Handles the WorkoutStarted event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Bike_WorkoutStarted(object sender, EventArgs e)
        {
            RunOnUIThread(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// Handles the WorkoutEnded event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Bike_WorkoutEnded(object sender, EventArgs e)
        {
            StatusText = "Workout Ended";
            IsPaused = false;
            pausedTimer.Stop();
            RunOnUIThread(CommandManager.InvalidateRequerySuggested);
            if (currentWorkout == null)
            {
                // Clear the display values
                RotationsPerMinute = 0;
                DistanceKms = 0;
                SpeedKph = 0;
                WorkoutTime = default;
            }
            else
            {
                // Save workout and display averages
                SaveWorkoutData();
                RotationsPerMinute = Convert.ToInt32(currentWorkout.AverageRotationsPerMinute);
                DistanceKms = Convert.ToDecimal(currentWorkout.DistanceKms);
                SpeedKph = Convert.ToDecimal(currentWorkout.AverageSpeedKph);
                WorkoutTime = new TimeSpan(0, 0, currentWorkout.Duration);
            }

            // Update today's distance
            todayDistanceKms = Convert.ToDecimal(context.Workouts.Where(w => w.StartDateTime.Date == DateTime.Today).Sum(w => w.DistanceKms));
            OnPropertyChanged(nameof(SummaryText));

            // Reset the context
            context.Dispose();
            context = new Context();
            currentWorkout = null;
        }

        /// <summary>
        /// Handles the WorkoutUpdate event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="BikeWorkoutEventArgs"/> instance containing the event data.</param>
        private void Bike_WorkoutUpdate(object sender, BikeWorkoutEventArgs e)
        {
            // Determine if the workout is paused
            IsPaused = e.Data.RotationsPerMinute == 0;

            if (IsPaused)
            {
                // Start the pause timer and update the status
                StatusText = "Paused";
                pausedTimer.Start();
            }
            else
            {
                // Show working up and save the data
                StatusText = "Working out";
                pausedTimer.Stop();
                UpdateWorkoutData(e.Data);
            }

            // Update the rest of the UI
            RotationsPerMinute = e.Data.RotationsPerMinute;
            DistanceKms = e.Data.DistanceKms;
            SpeedKph = e.Data.SpeedKph;
            WorkoutTime = e.Data.WorkoutTime;
            OnPropertyChanged(nameof(SummaryText));
            RunOnUIThread(CommandManager.InvalidateRequerySuggested);
        }

        /// <summary>
        /// Handles the Elapsed event of the PausedTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private async void PausedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // If paused for too long just stop the workout
            await bike.StopWorkout();
        }

        /// <summary>
        /// Handles the Elapsed event of the DailyTimer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void DailyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            todayDistanceKms = Convert.ToDecimal(context.Workouts.Where(w => w.StartDateTime.Date == DateTime.Today).Sum(w => w.DistanceKms));
            OnPropertyChanged(nameof(SummaryText));
        }

        /// <summary>
        /// Handles the Error event of the Bike control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExceptionEventArgs"/> instance containing the event data.</param>
        private void Bike_Error(object sender, ExceptionEventArgs e)
        {
            Error?.Invoke(this, e);
        }

        /// <summary>
        /// Updates the workout data.
        /// </summary>
        /// <param name="data">The data.</param>
        private void UpdateWorkoutData(BikeWorkoutData data)
        {
            // If no current workout, create one
            if (currentWorkout == null)
            {
                currentWorkout = new Workout();
                currentWorkout.StartDateTime = DateTime.Now;
                currentWorkout.EndDateTime = DateTime.Now;
                context.Add(currentWorkout);
            }

            // Update the current workout
            currentWorkout.Duration = Convert.ToInt32(data.WorkoutTime.TotalSeconds);
            currentWorkout.DistanceKms = decimal.ToDouble(data.DistanceKms);

            // Add the new entry
            currentWorkout.Entries.Add(new WorkoutEntry()
            {
                DistanceKms = decimal.ToDouble(data.DistanceKms),
                Second = data.Second,
                Timestamp = data.Timestamp,
                SpeedKph = decimal.ToDouble(data.SpeedKph),
                SpeedValue = data.SpeedValue,
                Duration = Convert.ToInt32(data.WorkoutTime.TotalSeconds),
                RotationsPerMinute = data.RotationsPerMinute
            });

            // Save the workout every 60 seconds
            if (currentWorkout.EndDateTime.AddSeconds(60) <= DateTime.Now)
            {
                SaveWorkoutData();
            }
        }

        /// <summary>
        /// Saves the workout.
        /// </summary>
        private void SaveWorkoutData()
        {
            currentWorkout.EndDateTime = DateTime.Now;
            context.SaveChanges();
        }

        /// <summary>
        /// Raises the error event.
        /// </summary>
        /// <param name="exception">The exception to raise.</param>
        /// <returns>True if error handled, false if not.</returns>
        private bool RaiseErrorEvent(Exception exception)
        {
            var eventArgs = new ExceptionEventArgs(exception);
            Error?.Invoke(this, eventArgs);
            return eventArgs.IsHandled;
        }
    }
}

// <copyright file="ScheduledTimer.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Codaris.Common
{
    /// <summary>
    /// A timer that fires at a particular time of day.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class ScheduledTimer : IDisposable
    {
        /// <summary>The timer.</summary>
        private readonly Timer timer = new();

        /// <summary>The trigger time.</summary>
        private readonly TimeSpan triggerTime;

        /// <summary>
        /// Occurs when timer has elapsed.
        /// </summary>
        public event ElapsedEventHandler? Elapsed;

        /// <summary>The disposed value.</summary>
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTimer"/> class.  This timer will trigger at the specified time of day.
        /// </summary>
        /// <param name="hour">The hour.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        public ScheduledTimer(int hour, int minute, int second)
        {
            triggerTime = new TimeSpan(hour, minute, second);
            timer.Elapsed += this.Timer_Elapsed;
            timer.Interval = CalculateInterval(triggerTime);
        }

        /// <summary>
        /// Handles the Elapsed event of the Timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Elapsed?.Invoke(this, e);
            if (AutoReset)
            {
                timer.Interval = CalculateInterval(triggerTime);
                timer.Start();
            }
        }

        /// <summary>
        /// Calculates the start interval from the trigger time and current date time.
        /// </summary>
        /// <param name="triggerTime">The trigger time.</param>
        /// <returns>The calculated interval in milliseconds.</returns>
        private static double CalculateInterval(TimeSpan triggerTime)
        {
            var now = DateTime.Now;
            var triggerData = new DateTime(now.Year, now.Month, now.Day, triggerTime.Hours, triggerTime.Minutes, triggerTime.Seconds);
            if (now > triggerData) triggerData = triggerData.AddDays(1);
            var interval = triggerData - now;
            if (interval <= TimeSpan.Zero) interval = TimeSpan.Zero;
            return interval.TotalMilliseconds;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should raise the Elapsed event only once (false) or repeatedly (true).
        /// </summary>
        public bool AutoReset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ScheduledTimer"/> should raise the Elapsed event.
        /// </summary>
        public bool Enabled { get => timer.Enabled; set => timer.Enabled = value; }

        /// <summary>
        /// Starts raising theElapsed event by setting Enabled to true.
        /// </summary>
        public void Start() => timer.Start();

        /// <summary>
        /// Stops raising the SElapsed event by setting Enabled to false.
        /// </summary>
        public void Stop() => timer.Stop();

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
                    timer.Dispose();
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
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

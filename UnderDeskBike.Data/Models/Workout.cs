// <copyright file="Workout.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnderDeskBike.Models
{
    /// <summary>
    /// The workout data entity.
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// Gets or sets the workout identifier.
        /// </summary>
        [Key]
        public int WorkoutId { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the duration.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the distance KMS.
        /// </summary>
        public double DistanceKms { get; set; }

        /// <summary>
        /// Gets or sets the average speed KPH.
        /// </summary>
        public double AverageSpeedKph { get; set; }

        /// <summary>
        /// Gets or sets the average rotations per minute.
        /// </summary>
        public double AverageRotationsPerMinute { get; set; }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        public virtual List<WorkoutEntry> Entries { get; } = [];

        /// <summary>
        /// Calculates the averages from the entries.
        /// </summary>
        public void AveragesFromEntries()
        {
            if (Entries.Count == 0) return;
            AverageRotationsPerMinute = Entries.Average(e => e.RotationsPerMinute);
            AverageSpeedKph = Entries.Average(e => e.SpeedKph);
        }
    }
}

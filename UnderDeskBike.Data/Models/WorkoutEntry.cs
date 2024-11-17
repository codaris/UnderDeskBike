// <copyright file="WorkoutEntry.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnderDeskBike.Models
{
    /// <summary>
    /// The workout sample entry.
    /// </summary>
    public class WorkoutEntry
    {
        /// <summary>
        /// Gets or sets the workout entry identifier.
        /// </summary>
        [Key]
        public int WorkoutEntryId { get; set; }

        /// <summary>
        /// Gets or sets the workout identifier.
        /// </summary>
        public int WorkoutId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the second.  This is a value from 0-59 from the bike.
        /// </summary>
        public int Second { get; set; }

        /// <summary>
        /// Gets or sets the distance in KMs.
        /// </summary>
        public double DistanceKms { get; set; }

        /// <summary>
        /// Gets or sets the workout time.
        /// </summary>
        public int Duration { get; set;  }

        /// <summary>
        /// Gets or sets the speed in KPH.
        /// </summary>
        public double SpeedKph { get; set; }

        /// <summary>
        /// Gets or sets the rotations per minute.
        /// </summary>
        public int RotationsPerMinute { get; set; }

        /// <summary>
        /// Gets or sets the speed value (0 to 9).
        /// </summary>
        public int SpeedValue { get; set; }

        /// <summary>
        /// Gets or sets the workout.
        /// </summary>
        [ForeignKey(nameof(WorkoutId))]
        public virtual Workout Workout { get; set; } = null!;
    }
}

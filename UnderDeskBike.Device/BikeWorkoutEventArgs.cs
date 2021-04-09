// <copyright file="BikeWorkoutEventArgs.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnderDeskBike
{
    /// <summary>
    /// The event data for the workout sample.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class BikeWorkoutEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the workout data.
        /// </summary>
        public BikeWorkoutData Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeWorkoutEventArgs"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        public BikeWorkoutEventArgs(BikeWorkoutData data)
        {
            Data = data;
        }
    }
}

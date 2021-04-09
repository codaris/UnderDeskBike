// <copyright file="BikeWorkoutData.cs" company="Wayne Venables">
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
    /// This a sample of workout data from the bike.
    /// </summary>
    /// <seealso cref="System.IEquatable{UnderDeskBike.BikeWorkoutData}" />
    public struct BikeWorkoutData : IEquatable<BikeWorkoutData>
    {
        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the second of the record.
        /// </summary>
        public int Second { get; }

        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        public decimal DistanceMiles { get; }

        /// <summary>
        /// Gets the distance in KMs.
        /// </summary>
        public decimal DistanceKms => DistanceMiles * 1.60934m;

        /// <summary>
        /// Gets the workout time.
        /// </summary>
        public TimeSpan WorkoutTime { get; }

        /// <summary>
        /// Gets the speed in MPH.
        /// </summary>
        public decimal SpeedMph { get; }

        /// <summary>
        /// Gets the speed in KPH.
        /// </summary>
        public decimal SpeedKph => SpeedMph * 1.60934m;

        /// <summary>
        /// Gets the rotations per minute.
        /// </summary>
        public int RotationsPerMinute { get; }

        /// <summary>
        /// Gets the speed value (0 to 9).
        /// </summary>
        public int SpeedValue { get; }

        /// <summary>
        /// Gets the unknown1 value.
        /// </summary>
        internal int Unknown1 { get; }

        /// <summary>
        /// Gets the unknown2 value.
        /// </summary>
        internal int Unknown2 { get; }

        /// <summary>
        /// Gets the unknown3 value.
        /// </summary>
        internal int Unknown3 { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeWorkoutData"/> struct.
        /// </summary>
        /// <param name="data">The data.</param>
        public BikeWorkoutData(byte[] data)
        {
            Timestamp = DateTime.Now;
            Second = data[0];
            DistanceMiles = ((data[1] << 8) + data[2]) / 100m;
            WorkoutTime = new TimeSpan(0, 0, (data[3] << 8) + data[4]);
            SpeedMph = ((data[7] << 8) + data[8]) / 10m;
            RotationsPerMinute = (data[10] << 8) + data[11];
            Unknown1 = data[15];
            SpeedValue = data[20];
            Unknown2 = data[30];
            Unknown3 = data[31];
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is BikeWorkoutData data && this.Equals(data);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return Timestamp.GetHashCode()
                ^ Second.GetHashCode()
                ^ DistanceMiles.GetHashCode()
                ^ WorkoutTime.GetHashCode()
                ^ SpeedMph.GetHashCode()
                ^ RotationsPerMinute.GetHashCode()
                ^ Unknown1.GetHashCode()
                ^ SpeedValue.GetHashCode()
                ^ Unknown2.GetHashCode()
                ^ Unknown3.GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(BikeWorkoutData other)
        {
            if (Timestamp != other.Timestamp) return false;
            if (Second != other.Second) return false;
            if (DistanceMiles != other.DistanceMiles) return false;
            if (WorkoutTime != other.WorkoutTime) return false;
            if (SpeedMph != other.SpeedMph) return false;
            if (RotationsPerMinute != other.RotationsPerMinute) return false;
            if (Unknown1 != other.Unknown1) return false;
            if (SpeedValue != other.SpeedValue) return false;
            if (Unknown2 != other.Unknown2) return false;
            if (Unknown3 != other.Unknown3) return false;
            return true;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(BikeWorkoutData lhs, BikeWorkoutData rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(BikeWorkoutData lhs, BikeWorkoutData rhs)
        {
            return !(lhs == rhs);
        }
    }
}

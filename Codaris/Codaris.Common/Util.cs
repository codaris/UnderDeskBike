// <copyright file="Util.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Codaris.Common
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Byte array equal comparison.
        /// </summary>
        /// <param name="a1">The a1.</param>
        /// <param name="a2">The a2.</param>
        /// <returns>Returns <c>true</c> if the specified arrays are equal.</returns>
        public static bool ByteArrayEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }
    }
}

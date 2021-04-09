// <copyright file="BikeDataException.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UnderDeskBike
{
    /// <summary>
    /// Raised when an error occurs receiving data from the bike.
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class BikeDataException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BikeDataException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BikeDataException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeDataException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public BikeDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeDataException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected BikeDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

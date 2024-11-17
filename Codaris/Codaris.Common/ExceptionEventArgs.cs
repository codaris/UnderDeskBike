// <copyright file="ExceptionEventArgs.cs" company="Wayne Venables">
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
    /// Exception raised event args.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception that caused the disconnection, if one exists.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is handled.
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionEventArgs" /> class.
        /// </summary>
        /// <param name="exception">The exception that caused the disconnection.</param>
        public ExceptionEventArgs(Exception exception)
        {
            var aggregateExcepton = (exception as AggregateException)?.Flatten();
            if (aggregateExcepton != null && aggregateExcepton.InnerExceptions.Count == 1)
            {
                Exception = aggregateExcepton.InnerException!;
            }
            else
            {
                Exception = exception;
            }
        }
    }
}

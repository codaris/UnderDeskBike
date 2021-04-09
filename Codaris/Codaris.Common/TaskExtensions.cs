// <copyright file="TaskExtensions.cs" company="Wayne Venables">
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
    /// Extensions for supporting async tasks.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Fires and forgets the task.  Requires an action to handle any task exceptions.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="errorHandler">The error handler.</param>
        public static async void FireAndForget(this Task task, Action<Exception> errorHandler)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errorHandler(ex);
            }
        }

        /// <summary>
        /// Fires and forgets the task.  Requires an action to handle any task exceptions.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="errorHandler">The error handler.</param>
        public static async void FireAndForget(this Task task, Func<Exception, bool> errorHandler)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!errorHandler(ex)) throw;
            }
        }
    }
}

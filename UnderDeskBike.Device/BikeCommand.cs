// <copyright file="BikeCommand.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace UnderDeskBike
{
    /// <summary>
    /// The base class for all bike commands.
    /// </summary>
    internal abstract class BikeCommand
    {
        /// <summary>
        /// Receives the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public abstract void Receive(byte[] data);
    }

    /// <summary>
    /// The base class for all bike commands.
    /// </summary>
    /// <typeparam name="T">The return type of the command.</typeparam>
    internal abstract class BikeCommand<T> : BikeCommand
    {
        /// <summary>
        /// The result completion source.
        /// </summary>
        private readonly TaskCompletionSource<T> resultCompletionSource = new TaskCompletionSource<T>();

        /// <summary>
        /// Sends this command using the specified send function.
        /// </summary>
        /// <param name="sendFunction">The send function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task Send(Func<byte[], Task<GattCommunicationStatus>> sendFunction);

        /// <summary>
        /// Gets the command result.
        /// </summary>
        public Task<T> Result => resultCompletionSource.Task;

        /// <summary>
        /// Sets the result.
        /// </summary>
        /// <param name="result">The result value.</param>
        protected void SetResult(T result)
        {
            resultCompletionSource.TrySetResult(result);
        }

        /// <summary>
        /// Sets the result as exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        protected void SetException(Exception exception)
        {
            resultCompletionSource.TrySetException(exception);
        }
    }
}

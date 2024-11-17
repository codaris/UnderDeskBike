// <copyright file="BikeConnectCommand.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Codaris.Common;

using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace UnderDeskBike
{
    /// <summary>
    /// The connect command.
    /// </summary>
    /// <seealso cref="UnderDeskBike.BikeCommand{bool}" />
    internal class BikeConnectCommand : BikeCommand<bool>
    {
        /// <summary>
        /// The connect packet.
        /// </summary>
        private static readonly byte[] ConnectPacket = [0xf9, 0xd0, 0x00, 0xc9];

        /// <summary>
        /// The result packet.
        /// </summary>
        private static readonly byte[] ResultPacket = [0xf9, 0xe0, 0x00, 0xd9];

        /// <summary>
        /// Sends this command using the specified send function.
        /// </summary>
        /// <param name="sendFunction">The send function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async override Task Send(Func<byte[], Task<GattCommunicationStatus>> sendFunction)
        {
            // Send the connect packet
            var result = await sendFunction(ConnectPacket);
            if (result != GattCommunicationStatus.Success)
            {
                SetResult(false);
            }
        }

        /// <summary>
        /// Receives the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Receive(byte[] data)
        {
            // Receive the result packet else error
            if (Util.ByteArrayEqual(ResultPacket, data)) SetResult(true);
            else SetException(new Exception("Unexpected result from bike"));
        }
    }
}

// <copyright file="BikeInfo1Command.cs" company="Wayne Venables">
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
    /// The first info command.  The official software sends this command and returns back information that has not be deciphiered.  It's not
    /// necessary to execute this command to all and this library does not call it at all.
    /// </summary>
    /// <seealso cref="UnderDeskBike.BikeCommand{bool}" />
    internal class BikeInfo1Command : BikeCommand<ValueTuple<int, int>>
    {
        /// <summary>The information packet.</summary>
        private static readonly byte[] InfoPacket = new byte[] { 0xf9, 0xd3, 0x0d, 0x01, 0x00, 0x00, 0x2c, 0x00, 0x00, 0x3c, 0x00, 0xa0, 0x00, 0x00, 0x00, 0x00, 0xe2 };

        /// <summary>
        /// The first result packet.
        /// </summary>
        private byte[] packet1 = null;

        /// <summary>
        /// The second result packet.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Not interpeting this data.")]
        private byte[] packet2 = null;

        /// <summary>
        /// Sends this command using the specified send function.
        /// </summary>
        /// <param name="sendFunction">The send function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async override Task Send(Func<byte[], Task<GattCommunicationStatus>> sendFunction)
        {
            // Send the connect packet
            var result = await sendFunction(InfoPacket);
            if (result != GattCommunicationStatus.Success)
            {
                SetException(new Exception("Send to bike failed"));
            }
        }

        /// <summary>
        /// Receives the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        public override void Receive(byte[] data)
        {
            if (data[0] != 0xF9)
            {
                SetException(new Exception("Unexpected result from bike"));
                return;
            }
            if (data[1] != 0xE3)
            {
                SetException(new Exception("Unexpected result from bike"));
                return;
            }
            if (packet1 == null) packet1 = data;
            else packet2 = data;

            // Otherwise construct result data
            SetResult((packet1[4], 0));
        }
    }
}

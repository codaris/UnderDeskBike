// <copyright file="BikeInfo2Command.cs" company="Wayne Venables">
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
    /// The second info command.  The official software sends this command and returns back information that has not be deciphiered.  It's not
    /// necessary to execute this command to all and this library does not call it at all.
    /// </summary>
    /// <seealso cref="UnderDeskBike.BikeCommand{bool}" />
    internal class BikeInfo2Command : BikeCommand<int>
    {
        /// <summary>The information packet.</summary>
        private static readonly byte[] InfoPacket = new byte[] { 0xf9, 0xd4, 0x0f, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x0f, 0x0c };

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
            if (data[1] != 0xE4)
            {
                SetException(new Exception("Unexpected result from bike"));
                return;
            }
            SetResult((data[4] << 8) + data[5]);
        }
    }
}

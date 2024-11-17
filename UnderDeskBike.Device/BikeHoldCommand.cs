// <copyright file="BikeHoldCommand.cs" company="Wayne Venables">
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
    /// The hold command.  After the connect command is sent, you must continuously send commands to the bike.  This command can
    /// be used to keep the connection option.  It doesn't appear to do anything else.
    /// </summary>
    /// <seealso cref="UnderDeskBike.BikeCommand{bool}" />
    internal class BikeHoldCommand : BikeCommand<byte[]>
    {
        /// <summary>The hold packet.</summary>
        private static readonly byte[] HoldPacket = [0xf9, 0xd1, 0x05, 0x02, 0x00, 0x00, 0x00, 0x00, 0xd1];

        /// <summary>
        /// The first result packet.
        /// </summary>
        private byte[]? packet1 = null;

        /// <summary>
        /// The second result packet.
        /// </summary>
        private byte[]? packet2 = null;

        /// <summary>
        /// Sends this command using the specified send function.
        /// </summary>
        /// <param name="sendFunction">The send function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async override Task Send(Func<byte[], Task<GattCommunicationStatus>> sendFunction)
        {
            // Send the connect packet
            var result = await sendFunction(HoldPacket);
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
            if (data.Length != 20)
            {
                SetException(new Exception("Short packet received from bike"));
                return;
            }
            if (data[0] != 0xF9)
            {
                SetException(new Exception("Unexpected result from bike"));
                return;
            }
            switch (data[1])
            {
                case 0xE1: packet1 = data; break;
                case 0xE2: packet2 = data; break;
                default:
                    SetException(new Exception("Unexpected result from bike"));
                    return;
            }

            // If not all packets returned wait for more
            if (packet1 == null || packet2 == null) return;

            // Otherwise construct result data
            var result = new byte[32];
            Buffer.BlockCopy(packet1, 4, result, 0, 16);
            Buffer.BlockCopy(packet2, 4, result, 16, 16);
            SetResult(result);
        }
    }
}

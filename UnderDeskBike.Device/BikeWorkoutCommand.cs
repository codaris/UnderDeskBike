// <copyright file="BikeWorkoutCommand.cs" company="Wayne Venables">
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
    /// The workout command.  This command is sent to start and continue a workout.  Each time this command is sent, the bike turns back a sample of
    /// workout data.
    /// </summary>
    /// <seealso cref="UnderDeskBike.BikeCommand{bool}" />
    internal class BikeWorkoutCommand : BikeCommand<BikeWorkoutData>
    {
        /// <summary>The start packet.</summary>
        private static readonly byte[] StartPacket = new byte[] { 0xf9, 0xd5, 0x0d, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdc };

        /// <summary>The continue packet.</summary>
        private static readonly byte[] ContinuePacket = new byte[] { 0xf9, 0xd5, 0x0d, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xdb };

        /// <summary>Sending start packet.</summary>
        private readonly bool start = false;

        /// <summary>The first result packet.</summary>
        private byte[] packet1 = null;

        /// <summary>The second result packet.</summary>
        private byte[] packet2 = null;

        /// <summary>The third result packet.</summary>
        private byte[] packet3 = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeWorkoutCommand"/> class.
        /// </summary>
        /// <param name="start">if set to <c>true</c> to start.</param>
        public BikeWorkoutCommand(bool start)
        {
            this.start = start;
        }

        /// <summary>
        /// Sends this command using the specified send function.
        /// </summary>
        /// <param name="sendFunction">The send function.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async override Task Send(Func<byte[], Task<GattCommunicationStatus>> sendFunction)
        {
            // Send the connect packet
            var result = await sendFunction(start ? StartPacket : ContinuePacket);
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
                SetException(new BikeDataException("Short packet received from bike"));
                return;
            }
            if (data[0] != 0xF9)
            {
                SetException(new BikeDataException("Unexpected result from bike"));
                return;
            }
            switch (data[1])
            {
                case 0xE5: packet1 = data; break;
                case 0xE6: packet2 = data; break;
                case 0xE7: packet3 = data; break;
                default:
                    SetException(new BikeDataException("Unexpected result from bike"));
                    return;
            }

            // If packet3 has been received and packets are missing, raise exception
            if (packet3 != null && (packet1 == null || packet2 == null))
            {
                SetException(new BikeDataException("Out of sync result from bike."));
            }

            // If not all packets returned wait for more
            if (packet1 == null || packet2 == null || packet3 == null) return;

            // Otherwise construct result data
            var workoutData = new byte[48];
            Buffer.BlockCopy(packet1, 4, workoutData, 0, 16);
            Buffer.BlockCopy(packet2, 4, workoutData, 16, 16);
            Buffer.BlockCopy(packet3, 4, workoutData, 32, 16);
            SetResult(new BikeWorkoutData(workoutData));
        }
    }
}

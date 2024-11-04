// <copyright file="BikeDevice.cs" company="Wayne Venables">
//     Copyright (c) 2021 Wayne Venables. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Codaris.Common;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace UnderDeskBike
{
    /// <summary>
    /// The low-level interface to the bike bluetooth device.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class BikeDevice : IDisposable
    {
        /// <summary>
        /// The service UUID.
        /// </summary>
        private const string UuidService = "6e400001-b5a3-f393-e0a9-e50e24dcca9e";

        /// <summary>
        /// The write UUID.
        /// </summary>
        private const string UuidWrite = "6e400002-b5a3-f393-e0a9-e50e24dcca9e";

        /// <summary>
        /// The read UUID.
        /// </summary>
        private const string UuidRead = "6e400003-b5a3-f393-e0a9-e50e24dcca9e";

        /// <summary>
        /// The disconnect packet.
        /// </summary>
        private static readonly byte[] DisconnectPacket = new byte[] { 65, 84, 58, 57, 57, 57, 57, 57, 57 };  // AT:999999

        /// <summary>
        /// Gets a value indicating whether this instance is listening.
        /// </summary>
        public bool IsListening => device != null;

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        public bool IsConnected => device?.ConnectionStatus == BluetoothConnectionStatus.Connected;

        /// <summary>
        /// Occurs when connected.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Occurs when disconnected.
        /// </summary>
        public event EventHandler Disconnected;

        /// <summary>
        /// The bluetooth LE device.
        /// </summary>
        private BluetoothLEDevice device = null;

        /// <summary>
        /// The gatt session.
        /// </summary>
        private GattSession gattSession = null;

        /// <summary>
        /// The device service.
        /// </summary>
        private GattDeviceService deviceService = null;

        /// <summary>
        /// The read characteristic.
        /// </summary>
        private GattCharacteristic readCharacteristic = null;

        /// <summary>
        /// The write characteristic.
        /// </summary>
        private GattCharacteristic writeCharacteristic = null;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly TextWriter logger = null;

        /// <summary>
        /// The current command.
        /// </summary>
        private BikeCommand currentCommand = null;

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private CancellationTokenSource commandCancellationTokenSource = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BikeDevice"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public BikeDevice(TextWriter logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Listen for a connection.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartListening()
        {
            try
            {
                // If already listening, do nothing
                if (IsListening) return;

                // Create device
                var deviceInfo = await GetBluetoothDeviceInfo();
                if (deviceInfo == null) throw new Exception("Under desk bluetooth device not found");
                device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                device.ConnectionStatusChanged += Device_ConnectionStatusChanged;

                // Initiate connection
                gattSession = await GattSession.FromDeviceIdAsync(device.BluetoothDeviceId);
                gattSession.MaintainConnection = true;
            }
            catch
            {
                // Stop the device and rethrow exception
                StopListening();
                throw;
            }
        }

        /// <summary>
        /// Stops the device from listening and resets.
        /// </summary>
        public void StopListening()
        {
            if (device != null)
            {
                OnDisconnect();
                gattSession?.Dispose();
                gattSession = null;
                if (device != null) device.ConnectionStatusChanged -= Device_ConnectionStatusChanged;
                device?.Dispose();
                device = null;
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <typeparam name="T">Return type of the command.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<T> ExecuteCommand<T>(BikeCommand<T> command)
        {
            try
            {
                currentCommand = command;
                commandCancellationTokenSource = new CancellationTokenSource();
                await command.Send(Send).ConfigureAwait(false);
                var task = await Task.WhenAny(command.Result, Task.Delay(2000));
                if (task == command.Result) return await command.Result;
                commandCancellationTokenSource?.Cancel();
                throw new TimeoutException("Timeout waiting for response from bike.");
            }
            finally
            {
                currentCommand = null;
            }
        }

        /// <summary>
        /// Devices the connection status changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private async void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            logger?.WriteLine("Connection Status Changed: " + sender.ConnectionStatus.ToString());
            if (IsConnected)
            {
                // Setup device parameters
                await OnConnect();
                Connected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnDisconnect();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when connected.
        /// </summary>
        private async Task OnConnect()
        {
            deviceService = device.GetGattService(Guid.Parse(UuidService));
            readCharacteristic = await GetCharacteristic(deviceService, Guid.Parse(UuidRead));
            writeCharacteristic = await GetCharacteristic(deviceService, Guid.Parse(UuidWrite));
            readCharacteristic.ValueChanged += ReadCharacteristic_ValueChanged;
            await readCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        /// <summary>
        /// Called when disconnecting.
        /// </summary>
        private void OnDisconnect()
        {
            commandCancellationTokenSource?.Cancel();
            writeCharacteristic = null;
            if (readCharacteristic != null) readCharacteristic.ValueChanged -= ReadCharacteristic_ValueChanged;
            readCharacteristic = null;
            deviceService?.Dispose();
            deviceService = null;
        }

        /// <summary>
        /// Gets the bike device.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task<DeviceInformation> GetBluetoothDeviceInfo()
        {
            var devices = await DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector());
            foreach (var device in devices) if (device.Name == "MCF-0000000000") return device;
            return null;
        }

        /// <summary>
        /// Gets the characteristic.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task<GattCharacteristic> GetCharacteristic(GattDeviceService service, Guid guid)
        {
            var result = await service.GetCharacteristicsForUuidAsync(guid);
            if (result.Status != GattCommunicationStatus.Success) throw new Exception("Connection error: " + result.Status.ToString());
            if (result.Characteristics.Count == 0) throw new Exception("Characteristic is not found");
            return result.Characteristics[0];
        }

        /// <summary>
        /// Sends the specified data to the bike.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<GattCommunicationStatus> Send(byte[] data)
        {
            logger?.WriteLine("SEND: " + string.Join(" ", data.Select(v => v.ToString("X2"))));
            using var writer = new DataWriter();
            writer.WriteBytes(data);
            return await writeCharacteristic.WriteValueAsync(writer.DetachBuffer()).AsTask(commandCancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the characteristic value changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GattValueChangedEventArgs"/> instance containing the event data.</param>
        private void ReadCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // An Indicate or Notify reported that the value has changed.
            using var reader = DataReader.FromBuffer(args.CharacteristicValue);
            var data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);
            if (logger != null)
            {
                logger.Write("RECV: ");
                foreach (var value in data) logger.Write(value.ToString("X2") + " ");
                logger.WriteLine();
            }

            if (data.Length >= DisconnectPacket.Length)
            {
                var checkPacket = new byte[DisconnectPacket.Length];
                System.Buffer.BlockCopy(data, 0, checkPacket, 0, checkPacket.Length);
                if (Util.ByteArrayEqual(data, DisconnectPacket)) return;
            }

            currentCommand?.Receive(data);
        }

        /// <summary>
        /// The disposed value.
        /// </summary>
        private bool disposedValue;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing) StopListening();
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1202:Elements should be ordered by access", Justification = "Dispose appears at end of class")]
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

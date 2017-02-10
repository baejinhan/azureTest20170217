using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager
{
    class DeviceManager
    {
        public DeviceManager()
        {
            deviceProcessor = new DevicesProcessor(DeviceManager.IoTHubConnectionString, 1000, "");
        }
        private DevicesProcessor deviceProcessor;

        public async Task<List<DeviceEntity>> GetDevices()
        {
            var devices = await deviceProcessor.GetDevices();
            devices.Sort();
            return devices;
        }

        public async Task<DeviceEntity> GetDevice(string DeviceID)
        {
            var device = await deviceProcessor.GetDevice(DeviceID);
            return device;
        }

        async public Task<DeviceEntity> RegisterDevice(string DeviceID)
        {
            var device = await deviceProcessor.RegisterDevice(DeviceID);

            return device;
        }

        public async void RemoveDevice(string DeviceID)
        {
            await deviceProcessor.RemoveDevice(DeviceID);
        }

        public async Task<DeviceEntity> ActivateDevice(string deviceID, DeviceStatus status)
        {
            return await deviceProcessor.ActivateDevice(deviceID, status);
        }

        private static string IoTHubConnectionString
        {
            get
            {
                return DeviceManager.sanitizeConnectionString(Properties.Settings.Default.IoTHubConnectionString);
            }
        }

        private static string sanitizeConnectionString(string connectionString)
        {
            return connectionString.Trim().Replace("\r", "").Replace("\n", "");
        }
    }
}

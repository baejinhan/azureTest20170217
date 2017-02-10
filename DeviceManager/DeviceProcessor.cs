using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager
{
    class DevicesProcessor
    {
        private List<DeviceEntity> listOfDevices;
        private RegistryManager registryManager;
        private String iotHubConnectionString;
        private int maxCountOfDevices;
        private String protocolGatewayHostName;

        public DevicesProcessor(string iotHubConnenctionString, int devicesCount, string protocolGatewayName)
        {
            this.listOfDevices = new List<DeviceEntity>();
            this.iotHubConnectionString = iotHubConnenctionString;
            this.maxCountOfDevices = devicesCount;
            this.protocolGatewayHostName = protocolGatewayName;
            this.registryManager = RegistryManager.CreateFromConnectionString(iotHubConnectionString);
        }

        async public Task RemoveDevice(string DeviceID)
        {
            await registryManager.RemoveDeviceAsync(DeviceID);
            await registryManager.CloseAsync();
        }

        public async Task<DeviceEntity> GetDevice(string DeviceID)
        {
            var device = await registryManager.GetDeviceAsync(DeviceID);
            DeviceEntity deviceEntity = GenerateDeviceEntity(device);

            if (device.Authentication != null &&
                      device.Authentication.SymmetricKey != null)
            {
                deviceEntity.PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey;
                deviceEntity.SecondaryKey = device.Authentication.SymmetricKey.SecondaryKey;
            }

            return deviceEntity;
        }

        public async Task<List<DeviceEntity>> GetDevices()
        {
            try
            {
                DeviceEntity deviceEntity;
                var devices = await registryManager.GetDevicesAsync(maxCountOfDevices);

                if (devices != null)
                {
                    foreach (var device in devices)
                    {
                        deviceEntity = GenerateDeviceEntity(device);

                        if (device.Authentication != null &&
                            device.Authentication.SymmetricKey != null)
                        {
                            deviceEntity.PrimaryKey = device.Authentication.SymmetricKey.PrimaryKey;
                            deviceEntity.SecondaryKey = device.Authentication.SymmetricKey.SecondaryKey;
                        }

                        listOfDevices.Add(deviceEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listOfDevices;
        }

        private DeviceEntity GenerateDeviceEntity(Device device)
        {
            return new DeviceEntity()
            {
                Id = device.Id,
                ConnectionState = device.ConnectionState.ToString(),
                ConnectionString = CreateDeviceConnectionString(device),
                LastActivityTime = device.LastActivityTime,
                LastConnectionStateUpdatedTime = device.ConnectionStateUpdatedTime,
                LastStateUpdatedTime = device.StatusUpdatedTime,
                MessageCount = device.CloudToDeviceMessageCount,
                State = device.Status.ToString(),
                SuspensionReason = device.StatusReason
            };
        }

        private String CreateDeviceConnectionString(Device device)
        {
            StringBuilder deviceConnectionString = new StringBuilder();

            var hostName = String.Empty;
            var tokenArray = iotHubConnectionString.Split(';');
            for (int i = 0; i < tokenArray.Length; i++)
            {
                var keyValueArray = tokenArray[i].Split('=');
                if (keyValueArray[0] == "HostName")
                {
                    hostName = tokenArray[i] + ';';
                    break;
                }
            }

            if (!String.IsNullOrWhiteSpace(hostName))
            {
                deviceConnectionString.Append(hostName);
                deviceConnectionString.AppendFormat("DeviceId={0}", device.Id);

                if (device.Authentication != null &&
                    device.Authentication.SymmetricKey != null)
                {
                    deviceConnectionString.AppendFormat(";SharedAccessKey={0}", device.Authentication.SymmetricKey.PrimaryKey);
                }

                if (this.protocolGatewayHostName.Length > 0)
                {
                    deviceConnectionString.AppendFormat(";GatewayHostName=ssl://{0}:8883", this.protocolGatewayHostName);
                }
            }

            return deviceConnectionString.ToString();
        }

        async public Task<DeviceEntity> ActivateDevice(string deviceID, DeviceStatus status)
        {
            var device = await registryManager.GetDeviceAsync(deviceID);
            device.Status = status;
            device = await registryManager.UpdateDeviceAsync(device);

            return GenerateDeviceEntity(device);
        }

        async public Task<DeviceEntity> RegisterDevice(string deviceID)
        {
            var device = new Device(deviceID);
            device = await registryManager.AddDeviceAsync(device);
            var deviceKey = new DeviceKeys();

            device.Authentication.SymmetricKey.PrimaryKey = deviceKey.PrimaryKey;
            device.Authentication.SymmetricKey.SecondaryKey = deviceKey.SecondaryKey;
            device.Status = DeviceStatus.Disabled;
            device = await registryManager.UpdateDeviceAsync(device);
            return GenerateDeviceEntity(device);
        }
    }

    internal class DeviceKeys
    {
        public string PrimaryKey { get { return _primaryKey; } }
        public string SecondaryKey { get { return _secondaryKey; } }
        private string _primaryKey;
        private string _secondaryKey;
        public DeviceKeys()
        {
            this._primaryKey = CryptoKeyGenerator.GenerateKey(32);
            this._secondaryKey = CryptoKeyGenerator.GenerateKey(32);
        }
    }
}

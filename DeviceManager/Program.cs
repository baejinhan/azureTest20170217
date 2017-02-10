using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //디바이스 관리자 생성
            DeviceManager deviceMgr = new DeviceManager();

            //디바이스 등록
            var device = deviceMgr.RegisterDevice("CESCO001");
            //디바이스 활성화
            deviceMgr.ActivateDevice(device.Result.Id, Microsoft.Azure.Devices.DeviceStatus.Enabled).Wait();

            deviceMgr.RegisterDevice("CESCO002").Wait();
            deviceMgr.ActivateDevice(device.Result.Id, Microsoft.Azure.Devices.DeviceStatus.Enabled).Wait();



            var deviceListJob = deviceMgr.GetDevices();

            //foreach (var item in deviceListJob.Result)
            //{
            //    deviceMgr.RemoveDevice(item.Id);
            //}

            //deviceListJob = deviceMgr.GetDevices();
            Console.WriteLine("================= registered items =============");
            foreach (var item in deviceListJob.Result)
            {
                if (item.State == "Disabled")
                {
                    deviceMgr.ActivateDevice(item.Id, Microsoft.Azure.Devices.DeviceStatus.Enabled).Wait();
                }
                //deviceMgr.RemoveDevice(item.Id);
                //Console.WriteLine(JsonConvert.SerializeObject(item));
            }

            deviceListJob = deviceMgr.GetDevices();
            foreach (var item in deviceListJob.Result)
            {
                //if (item.State == "Disabled")
                //{
                //    //deviceMgr.ActivateDevice(item.Id, Microsoft.Azure.Devices.DeviceStatus.Enabled).Wait();
                //}
                //deviceMgr.RemoveDevice(item.Id);
                Console.WriteLine(JsonConvert.SerializeObject(item));
            }

            Console.ReadLine();
        }
    }
}

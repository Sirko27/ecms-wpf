using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_Factories
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateMotor(string name, int maxSpeed)
        {
            return new MotorDevice(name, maxSpeed);
        }

        public IDevice CreatePump(string name, int maxSpeed)
        {
            return new PumpDevice(name, maxSpeed);
        }
    }
}

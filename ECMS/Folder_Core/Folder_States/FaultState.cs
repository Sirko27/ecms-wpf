using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_States
{
    public class FaultState : IDeviceStates
    {
        public string Name => "Fault";

        public void Start(MotorDevice device)
        {
            device.Reset();
        }

        public void Stop(MotorDevice device) { }
    }
}

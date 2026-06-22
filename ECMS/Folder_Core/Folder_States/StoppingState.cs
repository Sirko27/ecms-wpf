using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_States
{
    public class StoppingState : IDeviceStates
    {
        public string Name => "Stopping";

        public void Start(MotorDevice device) { }

        public void Stop(MotorDevice device) { }
    }
}

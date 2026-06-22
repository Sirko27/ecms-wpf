using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_States
{
    public class StoppedState : IDeviceStates
    {
        public string Name => "Stopped";

        public void Start(MotorDevice device)
        {
            device.SetState(new StartingState());
            _ = device.BeginStarting();
        }

        public void Stop(MotorDevice device) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_States
{
    public class NeedsRestartState : IDeviceStates
    {
        public string Name => "Needs Restart";

        public void Start(MotorDevice device) { }

        public void Stop(MotorDevice device)
        {
            device.ApplyPendingMaxSpeed();
            device.SetState(new StoppingState());
            _ = device.BeginStopping();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_States
{
    public interface IDeviceStates
    {
        string Name { get; }

        void Start(MotorDevice device);

        void Stop(MotorDevice device);
    }
}

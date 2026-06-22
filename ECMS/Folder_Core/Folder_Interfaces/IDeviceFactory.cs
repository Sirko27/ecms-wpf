using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_Interfaces
{
    public interface IDeviceFactory
    {
        IDevice CreateMotor(string name, int maxSpeed);
        IDevice CreatePump(string name, int maxSpeed);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public class DeviceSnapshot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxSpeed { get; set; }
        public string Type { get; set; }
        public string LinkedMotorId { get; set; }
    }

    public class SystemSnapshot
    {
        public List<DeviceSnapshot> Devices { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_Interfaces
{
    public interface IDevice
    {
        string ID { get; set; }
        string Name { get; set; }
        int MaxSpeed { get; set; }
        string State { get; }

        void Start();
        void Stop();
        void Update();

        event Action StateChanged;
        event Action DataChanged;
    }
}

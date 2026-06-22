using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public abstract class BaseDeviceViewModel : BaseViewModel
    {
        protected readonly IDevice _device;
        public IDevice _Device => _device;

        public string Name
        {
            get => _device.Name;
            set
            {
                _device.Name = value;
                OnPropertyChanged();
            }
        }

        public int MaxSpeed
        {
            get => _device.MaxSpeed;
            set
            {
                _device.MaxSpeed = value;
                OnPropertyChanged();
            }
        }

        public string State => _device.State;

        protected BaseDeviceViewModel(IDevice device)
        {
            _device = device;
            _device.StateChanged += () => OnPropertyChanged(nameof(State));
        }

        public virtual void Start()
        {
            _device.Start();
        }

        public virtual void Stop()
        {
            _device.Stop();
        }
    }
}

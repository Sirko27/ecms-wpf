using ECMS.Folder_Core;
using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public class PumpViewModel : BaseDeviceViewModel
    {
        private readonly PumpDevice _pump;

        public bool IsActive => _pump.IsActive;

        public string LinkedMotorName => _pump.LinkedMotor != null ? _pump.LinkedMotor.Name : "Не прив'язано";
        public bool IsLinked => _pump.LinkedMotor != null;

        public PumpViewModel(IDevice device) : base(device)
        {
            if (!(device is PumpDevice pump))
                throw new ArgumentException("Очікування PumpDevice", nameof(device));
            _pump = pump;

            _pump.StateChanged += () =>
            {
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(LinkedMotorName));
                OnPropertyChanged(nameof(IsLinked));
            };
        }

        public override void Start()
        {
            base.Start();
            OnPropertyChanged(nameof(IsActive));
        }

        public override void Stop()
        {
            _pump.Stop();
            OnPropertyChanged(nameof(IsActive));
        }

        public void NotifyLinkChanged()
        {
            OnPropertyChanged(nameof(LinkedMotorName));
            OnPropertyChanged(nameof(IsLinked));
        }
    }
}

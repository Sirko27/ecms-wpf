using ECMS.Folder_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public class MotorViewModel : BaseDeviceViewModel
    {
        private readonly MotorDevice _motor;

        public int Speed => _motor.Speed;
        public int Temperature => _motor.Temperature;

        public bool CanStart => _motor.State == "Stopped";
        public bool CanStop => _motor.State == "Running" || _motor.State == "Needs Restart";
        public bool CanReset => _motor.State == "Fault" || _motor.State == "Needs Restart";

        public bool NeedsRestart => _motor.State == "Needs Restart";

        public bool IsWarning => _motor.Temperature >= 180 && _motor.State != "Fault";
        public bool IsFaultTemp => _motor.State == "Fault";

        public MotorViewModel(MotorDevice motor) : base(motor)
        {
            _motor = motor;
            _motor.StateChanged += OnStateChanged;
            _motor.DataChanged += OnDataChanged;
            _motor.SpeedChanged += (_, __) => OnPropertyChanged(nameof(Speed));
        }

        public override void Start()
        {
            if (!CanStart)
                return;
            base.Start();
        }

        public override void Stop()
        {
            if (!CanStop)
                return;
            base.Stop();
        }

        public void Reset()
        {
            _motor.Reset();
            OnStateChanged();
            OnDataChanged();
        }

        private void OnStateChanged()
        {
            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            OnPropertyChanged(nameof(CanReset));
            OnPropertyChanged(nameof(NeedsRestart));
        }

        private void OnDataChanged()
        {
            OnPropertyChanged(nameof(Speed));
            OnPropertyChanged(nameof(Temperature));
            OnPropertyChanged(nameof(MaxSpeed));
            OnPropertyChanged(nameof(IsWarning));
        }
    }
}

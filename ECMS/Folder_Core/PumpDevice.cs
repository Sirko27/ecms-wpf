using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core
{
    public class PumpDevice : IDevice
    {
        private string _pumpState = "Stopped";
        private const int MaxAllowedSpeed = 300;
        private int _maxSpeed;

        public string ID { get; set; }
        public string Name { get; set; }

        public int MaxSpeed
        {
            get => _maxSpeed;
            set => _maxSpeed = Math.Min(Math.Max(value, 1), MaxAllowedSpeed);
        }

        public string State
        {
            get { return _pumpState; }
            set { }
        }

        public bool IsActive => _pumpState == "Running";

        public MotorDevice LinkedMotor { get; set; } = null;

        public event Action StateChanged;
        public event Action DataChanged;

        public PumpDevice(string name, int maxSpeed = 100)
        {
            ID = Guid.NewGuid().ToString("N").Substring(0, 8);
            Name = name;
            MaxSpeed = Math.Min(Math.Max(maxSpeed, 1), MaxAllowedSpeed);
        }

        public void Start()
        {
            if (_pumpState == "Stopped")
            {
                _pumpState = "Running";
                StateChanged?.Invoke();
            }
        }

        public void Stop()
        {
            if (_pumpState == "Running")
            {
                _pumpState = "Stopped";
                StateChanged?.Invoke();
            }
        }

        public void Update() { }
    }
}

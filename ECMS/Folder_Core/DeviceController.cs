using ECMS.Folder_Core.Folder_Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ECMS.Folder_Core
{
    public class DeviceController : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private readonly List<IDevice> _devices;
        private bool _isRunning;
        private bool _disposed;

        public IReadOnlyCollection<IDevice> Devices => _devices.AsReadOnly();
        public event EventHandler<IDevice> DeviceAdded;
        public event EventHandler<IDevice> DeviceRemoved;

        public DeviceController()
        {
            _devices = new List<IDevice>();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += OnTick;
        }

        public void AddDevice(IDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            _devices.Add(device);
            DeviceAdded?.Invoke(this, device);
        }

        public bool RemoveDevice(IDevice device)
        {
            if (_devices.Remove(device))
            {
                DeviceRemoved?.Invoke(this, device);
                return true;
            }

            return false;
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                _timer.Start();
            }
        }

        public void Stop()
        {
            if (_isRunning)
            {
                _isRunning = false;
                _timer.Stop();
            }
        }

        public void OnTick(object sender, EventArgs e)
        {
            foreach (var d in _devices)
            {
                d.Update();
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _timer.Tick -= OnTick;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

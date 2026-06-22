using ECMS.Folder_Core.Folder_Interfaces;
using ECMS.Folder_Core.Folder_States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace ECMS.Folder_Core
{
    public class MotorDevice : IDevice
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private IDeviceStates _currentState;

        private int _speed;
        private int _temperature;
        private int _maxSpeed;
        private string _name;
        private bool _warningFired = false;
        private int? _pendingMaxSpeed = null;
        private const double HeatPerSpeed = 0.65; // Коефіцієнт нагріву від швидкості
        private const double PumpCoolPerMax = 0.20; // Охолодження від насосів
        private const double PumpFalloff = 0.50;// Зменшення ефективності кожного наступного насоса
        private const int WarnHysteresis = 10;// Гістерезис для warning (щоб уникнути частих перемикань)
        private const int AmbientTemperature = 25; // Температура навколишнього середовища
        private const int WarningTemperature = 180; // Поріг warning температури
        private const int AlarmTemperature = 200; // Поріг аварійної температури
        private const double ThermalAlpha = 0.08; // Коефіцієнт згладжування температури

        private readonly List<PumpDevice> _linkedPump = new List<PumpDevice>();
        public IReadOnlyList<PumpDevice> LinkedPumps => _linkedPump.AsReadOnly();
        public string ID { get; set; }

        public string State
        {
            get
            {
                return _currentState?.Name ?? "Unknown";
            }
            set { }
        }

        public int Temperature => _temperature;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                DataChanged?.Invoke();
            }
        }

        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                int newValue = Math.Max(1, value);

                if ((State == "Running" || State == "Starting" || State == "Stopping") && newValue != _maxSpeed)
                {
                    _pendingMaxSpeed = newValue;
                    SetState(new NeedsRestartState());
                    return;
                }

                _maxSpeed = newValue;
                _pendingMaxSpeed = null;

                if (_maxSpeed < _speed)
                    Speed = _maxSpeed;
            }
        }

        public int Speed
        {
            get { return _speed; }
            set
            {
                int clamped = Math.Max(0, Math.Min(value, _maxSpeed));

                if (_speed != clamped)
                {
                    _speed = clamped;
                    SpeedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event Action StateChanged;
        public event Action DataChanged;
        public event EventHandler SpeedChanged;

        public event Action<string> WarningTriggered;
        public event Action<string> AlarmTriggered;

        public MotorDevice(string name, int maxSpeed = 100)
        {
            ID = Guid.NewGuid().ToString("N").Substring(0, 8);
            _name = name;
            _maxSpeed = Math.Max(1, maxSpeed);
            _temperature = 25;
            _currentState = new StoppedState();
        }

        public void LinkPump(PumpDevice pump)
        {
            if (pump != null && !_linkedPump.Contains(pump))
            {
                _linkedPump.Add(pump);
            }
        }

        public void UnlinkPump(PumpDevice pump)
        {
            if (pump != null)
            {
                _linkedPump.Remove(pump);
            }
        }

        public void SetState(IDeviceStates newState)
        {
            _currentState = newState;
            StateChanged?.Invoke();
        }

        public void Start() => _currentState.Start(this);
        public void Stop() => _currentState.Stop(this);

        public void Update()
        {
            UpdateTemperature();
            CheckTemperatureAlarms();
            DataChanged?.Invoke();
        }

        private void UpdateTemperature()
        {
            bool isActive = State == "Running" || State == "Starting" || State == "Stopping" || State == "Needs Restart";
            double targetTemp = isActive ? AmbientTemperature + _speed * HeatPerSpeed : AmbientTemperature;

            if (isActive && _linkedPump.Count > 0)
            {
                double efficiency = 1.0;

                foreach (var pump in _linkedPump)
                {
                    if (!pump.IsActive) continue;

                    targetTemp -= pump.MaxSpeed * PumpCoolPerMax * efficiency;
                    efficiency *= PumpFalloff;
                }

                targetTemp = Math.Max(AmbientTemperature, targetTemp);
            }

            double delta = targetTemp - _temperature;
            double alpha = (isActive && delta > 20) ? ThermalAlpha * 2.5 : ThermalAlpha;
            _temperature = (int)Math.Round(_temperature + alpha * delta);

            if (_temperature < AmbientTemperature) _temperature = AmbientTemperature;
        }

        private void CheckTemperatureAlarms()
        {
            if (_temperature >= AlarmTemperature)
            {
                AlarmTriggered?.Invoke($"{_name}: аварійна температура ({_temperature} °C) двигун зупинено");
                EventLogger.Instance.Log(_name, $"ALARM: {_temperature} °C");
                TriggerOverheatFault();
                return;
            }

            if (_temperature >= WarningTemperature && !_warningFired)
            {
                _warningFired = true;
                WarningTriggered?.Invoke($"{_name}: висока температура ({_temperature} °C)");
                EventLogger.Instance.Log(_name, $"WARNING: {_temperature} °C");
            }

            else if (_temperature < WarningTemperature - WarnHysteresis)
                _warningFired = false;
        }

        private void TriggerOverheatFault()
        {
            CancelAndResetCts();
            Speed = 0;
            SetState(new FaultState());
            DataChanged?.Invoke();
        }

        public void Reset()
        {
            if (State != "Fault" && State != "Needs Restart")
                return;

            if (State == "Needs Restart")
            {
                _ = ResetWithRetartAsync();
                return;
            }

            CancelAndResetCts();
            _warningFired = false;
            Speed = 0;
            SetState(new StoppedState());
        }

        private async Task ResetWithRetartAsync()
        {
            CancelAndResetCts();
            var token = _cts.Token;

            SetState(new StoppingState());
            await BeginStoppingCore(token);

            if (State == "Stopped")
            {
                ApplyPendingMaxSpeed();
                SetState(new StartingState());
                await BeginStartingCore(token);
            }
        }

        public async Task BeginStarting()
        {
            CancelAndResetCts();
            await BeginStartingCore(_cts.Token);
        }

        public async Task BeginStopping()
        {
            CancelAndResetCts();
            await BeginStoppingCore(_cts.Token);
        }
        private async Task BeginStartingCore(CancellationToken token)
        {
            try
            {
                while (_speed < _maxSpeed)
                {
                    await Task.Delay(100, token);
                    Speed += 5;
                    DataChanged?.Invoke();
                }
                SetState(new RunningState());
            }
            catch (OperationCanceledException) { }
        }

        private async Task BeginStoppingCore(CancellationToken token)
        {
            try
            {
                while (_speed > 0)
                {
                    await Task.Delay(100, token);
                    Speed -= 5;
                    DataChanged?.Invoke();
                }
                SetState(new StoppedState());
            }
            catch (OperationCanceledException) { }
        }

        private void CancelAndResetCts()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
        }

        public void ApplyPendingMaxSpeed()
        {
            if (_pendingMaxSpeed.HasValue)
            {
                _maxSpeed = Math.Max(1, _pendingMaxSpeed.Value);
                _pendingMaxSpeed = null;

                if (_maxSpeed < _speed)
                    Speed = _maxSpeed;

                DataChanged?.Invoke();
                StateChanged?.Invoke();
            }
        }
    }
}

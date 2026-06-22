using ECMS.Folder_Core;
using ECMS.Folder_Core.Folder_Interfaces;
using ECMS.Folder_UI.Folder_Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private int _motorCount = 0;
        private int _pumpCount = 0;

        private readonly DeviceController _controller;
        private readonly IDialogService _dialogService;
        private readonly IDeviceFactory _deviceFactory;
        private readonly EventLogger _logger = EventLogger.Instance;
        private readonly SnapshotManager _snapshotManager = new SnapshotManager();

        private RelayCommand _startCommand;
        private RelayCommand _stopCommand;
        private RelayCommand _openSettingsCommand;
        private RelayCommand _removeCommand;

        public ObservableCollection<BaseDeviceViewModel> Devices { get; }
        public ObservableCollection<string> LogEntries { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Alarms { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Warnings { get; } = new ObservableCollection<string>();

        private BaseDeviceViewModel _selectedDevice;

        public BaseDeviceViewModel SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice != value)
                {
                    _selectedDevice = value;
                    OnPropertyChanged();
                    _startCommand?.RaiseCanExecuteChanged();
                    _stopCommand?.RaiseCanExecuteChanged();
                    _openSettingsCommand?.RaiseCanExecuteChanged();
                    _removeCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddMotorCommand { get; }
        public ICommand AddPumpCommand { get; }
        public ICommand StartCommand => _startCommand;
        public ICommand StopCommand => _stopCommand;
        public ICommand OpenSettingsCommand => _openSettingsCommand;
        public ICommand ResetCommand { get; }
        public ICommand RemoveCommand => _removeCommand;
        public ICommand SaveConfigCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand LinkPumpCommand { get; }
        public ICommand UnlinkPumpCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public MainViewModel(IDialogService dialogService, IDeviceFactory deviceFactory)
        {
            _controller = new DeviceController();
            _dialogService = dialogService;
            _deviceFactory = deviceFactory;

            _controller.Start();
            Devices = new ObservableCollection<BaseDeviceViewModel>();

            AddMotorCommand = new RelayCommand(AddMotor);
            AddPumpCommand = new RelayCommand(AddPump);

            _openSettingsCommand = new RelayCommand(
                OpenSettings,
                () => SelectedDevice != null);

            _startCommand = new RelayCommand(
                () => SelectedDevice?.Start(),
                () => SelectedDevice is MotorViewModel m && m.CanStart || SelectedDevice is PumpViewModel p && p.State == "Stopped");

            _stopCommand = new RelayCommand(
                () => SelectedDevice?.Stop(),
                () => SelectedDevice is MotorViewModel m && m.CanStop || SelectedDevice is PumpViewModel p && p.State == "Running");

            ResetCommand = new RelayCommand(
                () =>
                {
                    if (SelectedDevice is MotorViewModel mvm)
                        mvm.Reset();
                },
                () => SelectedDevice is MotorViewModel mv && mv.CanReset);

            _removeCommand = new RelayCommand(
                RemoveSelected,
                () => SelectedDevice != null);

            SaveConfigCommand = new RelayCommand(SaveConfig);
            LoadConfigCommand = new RelayCommand(LoadConfig);

            _logger.EntryAdded += entry =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    LogEntries.Add(entry);
                    OnPropertyChanged(nameof(LogEntries));
                });
            };

            LinkPumpCommand = new RelayCommand(
                LinkSelectedPump,
                () => SelectedDevice is PumpViewModel pv && !pv.IsLinked);

            UnlinkPumpCommand = new RelayCommand(
                UnlinkSelectedPump,
                () => SelectedDevice is PumpViewModel pv && pv.IsLinked);

            UndoCommand = new RelayCommand(Undo, () => _snapshotManager.CanUndo);
            RedoCommand = new RelayCommand(Redo, () => _snapshotManager.CanRedo);
        }

        private string GenerateUniqueName(string baseName)
        {
            var existingName = Devices.Select(d => d.Name).ToHashSet();

            if (!existingName.Contains(baseName)) return baseName;

            int i = 2;
            while (existingName.Contains($"{baseName} ({i})"))
                i++;
            return $"{baseName} ({i})";
        }

        private void AddMotor()
        {
            _snapshotManager.Push(CaptureSnapshot());

            string name = GenerateUniqueName("Motor " + (_motorCount + 1));
            var motor = (MotorDevice)_deviceFactory.CreateMotor(name, maxSpeed: 100);
            _motorCount++;

            motor.AlarmTriggered += msg =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Alarms.Add($"{DateTime.Now:HH:mm:ss} {msg}");
                });
            };

            motor.WarningTriggered += msg =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    Warnings.Add($"{DateTime.Now:HH:mm:ss} {msg}");
                });
            };

            _controller.AddDevice(motor);
            var vm = new MotorViewModel(motor);
            Devices.Add(vm);
            _logger.Log(motor.Name, "Motor added");
        }

        private void AddPump()
        {
            _snapshotManager.Push(CaptureSnapshot());

            string name = GenerateUniqueName("Pump " + (_pumpCount + 1));
            var pump = (PumpDevice)_deviceFactory.CreatePump(name, maxSpeed: 50);
            _pumpCount++;

            _controller.AddDevice(pump);
            var vm = new PumpViewModel(pump);
            Devices.Add(vm);
            _logger.Log(pump.Name, "Pump added");
        }

        private void RemoveSelected()
        {
            if (SelectedDevice == null)
                return;

            var deviceName = SelectedDevice.Name;
            var device = SelectedDevice._Device;

            if (device is PumpDevice p)
            {
                if (p.LinkedMotor != null)
                {
                    var pumpName = p.LinkedMotor.Name;
                    p.LinkedMotor.UnlinkPump(p);
                    p.LinkedMotor = null;
                    _logger.Log(p.Name, $"Unlinked from {pumpName}");
                }
            }

            else if (device is MotorDevice m)
            {
                foreach (var linkedPump in m.LinkedPumps.ToList())
                {
                    linkedPump.LinkedMotor = null;
                    _logger.Log(linkedPump.Name, $"Unlinked from {m.Name}");
                }
            }

            _snapshotManager.Push(CaptureSnapshot());
            _controller.RemoveDevice(device);
            Devices.Remove(SelectedDevice);
            _logger.Log(deviceName, "Device removed");
            SelectedDevice = null;
        }

        private void OpenSettings()
        {
            if (SelectedDevice == null) return;

            var existingNames = Devices.Select(d => d.Name);
            bool isPump = SelectedDevice is PumpViewModel;
            var viewModel = new SettingViewModel(SelectedDevice.Name, SelectedDevice.MaxSpeed, isPump, existingNames);

            if (_dialogService.ShowSettingDialog(viewModel) == true)
            {
                _snapshotManager.Push(CaptureSnapshot());
                SelectedDevice.Name = viewModel.Name;
                SelectedDevice.MaxSpeed = viewModel.MaxSpeed;
                _logger.Log(SelectedDevice.Name, "Settings updated");
            }
        }

        private void SaveConfig()
        {
            ConfigRepository.Save(_controller.Devices);
            _logger.Log("System", "Configuration saved");
        }

        private void LoadConfig()
        {
            foreach (var vm in Devices.ToList())
                _controller.RemoveDevice(vm._Device);
            Devices.Clear();
            _motorCount = 0;
            _pumpCount = 0;


            var saved = ConfigRepository.Load();

            var motorMap = new Dictionary<string, MotorDevice>();
            var pumpMap = new Dictionary<string, PumpDevice>();

            foreach (var data in saved)
            {
                if (data.Type == "Motor")
                {
                    var m = (MotorDevice)_deviceFactory.CreateMotor(data.Name, data.MaxSpeed);
                    _controller.AddDevice(m);
                    Devices.Add(new MotorViewModel(m));
                    motorMap[data.Name] = m;
                    _motorCount++;
                }
                else if (data.Type == "Pump")
                {
                    var p = (PumpDevice)_deviceFactory.CreatePump(data.Name, data.MaxSpeed);
                    _controller.AddDevice(p);
                    Devices.Add(new PumpViewModel(p));
                    pumpMap[data.Name] = p;
                    _pumpCount++;
                }
            }

            foreach (var data in saved.Where(d => d.Type == "Motor"))
            {
                if (!motorMap.TryGetValue(data.Name, out var motor))
                    continue;

                foreach (var pumpName in data.LinkedPumpNames)
                {
                    if (pumpMap.TryGetValue(pumpName, out var pump))
                    {
                        motor.LinkPump(pump);
                        pump.LinkedMotor = motor;
                        var pvm = Devices.OfType<PumpViewModel>().FirstOrDefault(p => p.Name == pumpName);
                        pvm?.NotifyLinkChanged();
                    }
                }
            }

            _logger.Log("System", "Configuration loaded");
        }

        private void LinkSelectedPump()
        {
            if (!(SelectedDevice is PumpViewModel pvm)) return;

            var motors = Devices.OfType<MotorViewModel>().ToList();
            if (!motors.Any())
            {
                _logger.Log("System", "No motors available to link");
                return;
            }

            var viewModel = new LinkPumpViewModel(motors);
            if (_dialogService.ShowLinkPumpDialog(viewModel) == true && viewModel.SelectedMotor != null)
            {
                var motorDevice = (MotorDevice)viewModel.SelectedMotor._Device;
                var pumpDevice = (PumpDevice)pvm._Device;

                motorDevice.LinkPump(pumpDevice);
                pumpDevice.LinkedMotor = motorDevice;
                pvm.NotifyLinkChanged();

                _logger.Log(pvm.Name, $"Linked to {motorDevice.Name}");
            }
        }

        private void UnlinkSelectedPump()
        {
            if (!(SelectedDevice is PumpViewModel pvm)) return;

            var pumpDevice = (PumpDevice)pvm._Device;

            if (pumpDevice.LinkedMotor != null)
            {
                var motorName = pumpDevice.LinkedMotor.Name;
                pumpDevice.LinkedMotor.UnlinkPump(pumpDevice);
                pumpDevice.LinkedMotor = null;
                pvm.NotifyLinkChanged();
                _logger.Log(pvm.Name, $"Unlinked from {motorName}");
            }
        }

        private SystemSnapshot CaptureSnapshot()
        {
            var snapshots = Devices.Select(vm => new DeviceSnapshot
            {
                Id = vm._Device is MotorDevice m ? m.ID : vm._Device is PumpDevice p ? p.ID : "",
                Name = vm.Name,
                MaxSpeed = vm.MaxSpeed,
                Type = vm is MotorViewModel ? "Motor" : "Pump",
                LinkedMotorId = vm._Device is PumpDevice pd && pd.LinkedMotor != null ? pd.LinkedMotor.ID : null
            }).ToList();
            return new SystemSnapshot { Devices = snapshots };
        }

        private void Undo()
        {
            var snapshot = _snapshotManager.Undo(CaptureSnapshot());
            RestoreSnapshot(snapshot);
        }

        private void Redo()
        {
            var snapshot = _snapshotManager.Redo(CaptureSnapshot());
            RestoreSnapshot(snapshot);
        }

        private void RestoreSnapshot(SystemSnapshot snapshot)
        {
            foreach (var vm in Devices.ToList())
            {
                vm._Device.Stop();
                _controller.RemoveDevice(vm._Device);
            }
            Devices.Clear();

            var motorMap = new Dictionary<string, MotorDevice>();
            var pumpMap = new Dictionary<string, PumpDevice>();

            foreach (var s in snapshot.Devices)
            {
                if (s.Type == "Motor")
                {
                    var m = (MotorDevice)_deviceFactory.CreateMotor(s.Name, s.MaxSpeed);
                    m.ID = s.Id;
                    m.AlarmTriggered += msg =>
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            Alarms.Add($"{DateTime.Now:HH:mm:ss} {msg}");
                        });
                    };

                    m.WarningTriggered += msg =>
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            Warnings.Add($"{DateTime.Now:HH:mm:ss} {msg}");
                        });
                    };

                    _controller.AddDevice(m);
                    Devices.Add(new MotorViewModel(m));
                    motorMap[s.Id] = m;
                }
                else
                {
                    var p = (PumpDevice)_deviceFactory.CreatePump(s.Name, s.MaxSpeed);
                    p.ID = s.Id;
                    _controller.AddDevice(p);
                    var pvm = new PumpViewModel(p);
                    Devices.Add(pvm);
                    pumpMap[s.Id] = p;
                }
            }

            foreach (var s in snapshot.Devices.Where(d => d.LinkedMotorId != null))
            {
                if (pumpMap.TryGetValue(s.Id, out var pump) && motorMap.TryGetValue(s.LinkedMotorId, out var motor))
                {
                    motor.LinkPump(pump);
                    pump.LinkedMotor = motor;
                    Devices.OfType<PumpViewModel>().FirstOrDefault(pv => pv._Device == pump)?.NotifyLinkChanged();
                }
            }
            _logger.Log("System", "State restored");
        }
    }
}

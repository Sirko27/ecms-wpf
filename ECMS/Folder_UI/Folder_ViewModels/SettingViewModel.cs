using ECMS.Folder_UI.Folder_Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        private string _name;
        private int _maxSpeed;
        private string _errorName;
        private string _errorMaxSpeed;
        private readonly bool _isPump;
        private string _currentName;

        private HashSet<string> _existingNames;
        private int MaxSpeedLimit => _isPump ? 300 : 10000;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    ValidateName();
                }
            }
        }

        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                if (_maxSpeed != value)
                {
                    _maxSpeed = value;
                    OnPropertyChanged();
                    ValidateMaxSpeed();
                }
            }
        }

        public string ErrorName
        {
            get => _errorName;
            private set
            {
                _errorName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public string ErrorMaxSpeed
        {
            get => _errorMaxSpeed;
            private set
            {
                _errorMaxSpeed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => string.IsNullOrEmpty(ErrorName) && string.IsNullOrEmpty(ErrorMaxSpeed);
        public Action<bool> CloseAction { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public SettingViewModel(string currentName, int currentMaxSpeed, bool isPump = false, IEnumerable<string> existingNames = null)
        {
            _isPump = isPump;
            _name = currentName;
            _maxSpeed = currentMaxSpeed;

            _currentName = currentName;
            _existingNames = existingNames?.ToHashSet() ?? new HashSet<string>();

            SaveCommand = new RelayCommand(Save, () => IsValid);
            CancelCommand = new RelayCommand(Cancel);

            ValidateName();
            ValidateMaxSpeed();
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                ErrorName = "Enter Name";
            else if (Name != _currentName && _existingNames.Contains(Name))
                ErrorName = "Device with name alread exists";
            else
                ErrorName = null;
        }

        private void ValidateMaxSpeed()
        {
            ErrorMaxSpeed = (_maxSpeed < 1 || _maxSpeed > MaxSpeedLimit)
                ? $"Макс. швидкість повинна бути від 1 до {MaxSpeedLimit}"
                : null;
        }

        private void Save() => CloseAction?.Invoke(true);
        private void Cancel() => CloseAction?.Invoke(false);
    }
}

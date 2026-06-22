using ECMS.Folder_UI.Folder_Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ECMS.Folder_UI.Folder_ViewModels
{
    public class LinkPumpViewModel : BaseViewModel
    {
        private readonly IEnumerable<MotorViewModel> _motors;
        private MotorViewModel _selectedMotor;

        public IEnumerable<MotorViewModel> Motors => _motors;

        public MotorViewModel SelectedMotor
        {
            get => _selectedMotor;
            set
            {
                if (_selectedMotor != value)
                {
                    _selectedMotor = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanLink));
                }
            }
        }

        public bool CanLink => SelectedMotor != null;

        public ICommand LinkCommand { get; }
        public ICommand CancelCommand { get; }

        public Action<bool> CloseAction { get; set; }

        public LinkPumpViewModel(IEnumerable<MotorViewModel> motors)
        {
            _motors = motors;
            LinkCommand = new RelayCommand(Link, () => CanLink);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Link()
        {
            CloseAction?.Invoke(true);
        }

        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }
    }
}

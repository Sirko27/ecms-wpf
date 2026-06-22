using ECMS.Folder_UI.Folder_ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ECMS.Folder_UI.Folder_Views
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(SettingViewModel viewModel)// дізнатися
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.CloseAction = result =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}

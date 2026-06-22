using ECMS.Folder_Core.Folder_Factories;
using ECMS.Folder_UI.Folder_Services;
using ECMS.Folder_UI.Folder_ViewModels;
using ECMS.Folder_UI.Folder_Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ECMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var factory = new DeviceFactory();
            var dialogService = new DialogService();
            var viewModel = new MainViewModel(dialogService, factory);

            var window = new MainWindow();
            window.DataContext = viewModel;
            window.Show();
        }
    }
}

using ECMS.Folder_Core.Folder_Interfaces;
using ECMS.Folder_UI.Folder_ViewModels;
using ECMS.Folder_UI.Folder_Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_UI.Folder_Services
{
    public class DialogService : IDialogService
    {
        public bool? ShowSettingDialog(object viewModel)
        {
            var dialog = new SettingWindow((SettingViewModel)viewModel);
            return dialog.ShowDialog();
        }

        public bool? ShowLinkPumpDialog(object viewModel)
        {
            var dialog = new LinkPumpWindow((LinkPumpViewModel)viewModel);
            return dialog.ShowDialog();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Folder_Core.Folder_Interfaces
{
    public interface IDialogService
    {
        bool? ShowSettingDialog(object viewModel);
        bool? ShowLinkPumpDialog(object viewModel);
    }
}

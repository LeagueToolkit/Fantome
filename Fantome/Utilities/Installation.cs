using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.ViewModels;
using Fantome.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;

namespace Fantome.Utilities
{
    public static class Installation
    {
        public static async Task<object> Install(ModFile mod, ModManager modManager)
        {
            InstallingModDialog installingModDialog = new InstallingModDialog()
            {
                DataContext = new InstallingModViewModel(mod, modManager)
            };

            return await DialogHost.Show(installingModDialog, "RootDialog", installingModDialog.StartInstallation, null);
        }
    }
}

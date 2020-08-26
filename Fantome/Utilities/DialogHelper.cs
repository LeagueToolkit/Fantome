using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.ModManagement;
using Fantome.ModManagement.IO;
using Fantome.MVVM.ViewModels;
using Fantome.MVVM.ModelViews.Dialogs;
using MaterialDesignThemes.Wpf;
using Fantome.MVVM.ViewModels.CreateMod;

namespace Fantome.Utilities
{
    public static class DialogHelper
    {
        public static DialogHost MessageDialog { get; set; }
        public static DialogHost OperationDialog { get; set; }
        public static DialogHost RootDialog { get; set; }

        public static async void ShowModContentValidationDialog(string folderType)
        {
            ModContentValidationDialog dialog = new ModContentValidationDialog(folderType);

            await DialogHost.Show(dialog, "MessageDialog");
        }

        public static async Task<object> ShowAssetConflictDialog(ModFile mod, List<string> collisions)
        {
            AssetCollisionDialog dialog = new AssetCollisionDialog(mod, collisions);

            return await DialogHost.Show(dialog, "MessageDialog");
        }

        public static async Task ShowMessageDialog(string message)
        {
            MessageDialog dialog = new MessageDialog(message);

            await DialogHost.Show(dialog, "MessageDialog");
        }

        public static async Task<object> ShowGenerateWadFilesDialog(ModFile mod, LeagueFileIndex index)
        {
            GeneratingWadFilesDialog dialog = new GeneratingWadFilesDialog(mod, index);

            return await DialogHost.Show(dialog, "OperationDialog", dialog.StartGeneration, null);
        }

        public static async Task<object> ShowInstallModDialog(ModFile mod, ModManager modManager)
        {
            InstallingModDialog dialog = new InstallingModDialog()
            {
                DataContext = new InstallingModViewModel(mod, modManager)
            };

            return await DialogHost.Show(dialog, "OperationDialog", dialog.StartInstallation, null);
        }

        public static async Task<object> ShowUninstallModDialog(ModFile mod, ModManager modManager)
        {
            UninstallingModDialog dialog = new UninstallingModDialog()
            {
                DataContext = new UninstallingModViewModel(mod, modManager)
            };

            return await DialogHost.Show(dialog, "OperationDialog", dialog.StartUninstallation, null);
        }

        public static async Task<ModFile> ShowCreateModDialog(LeagueFileIndex index)
        {
            CreateModDialogViewModel dialogModel = new CreateModDialogViewModel(index);
            CreateModDialog dialog = new CreateModDialog(index, dialogModel);

            object result = await DialogHost.Show(dialog, "RootDialog", (dialog.DataContext as CreateModDialogViewModel).ClosingEventHandler);
            if ((bool)result)
            {
                return dialogModel.GetCreatedMod();
            }
            else
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.UserControls.Dialogs;
using MaterialDesignThemes.Wpf;

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

        public static async void ShowMessageDialog(string message)
        {
            MessageDialog dialog = new MessageDialog(message);

            await DialogHost.Show(dialog, "MessageDialog");
        }
    }
}

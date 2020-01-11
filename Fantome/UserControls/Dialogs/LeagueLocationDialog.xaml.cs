using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Fantome.MVVM.ViewModels;
using Fantome.Utilities;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fantome.UserControls.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectLeagueLocationDialog.xaml
    /// </summary>
    public partial class LeagueLocationDialog : UserControl
    {
        public LeagueLocationDialogViewModel ViewModel { get => this.DataContext as LeagueLocationDialogViewModel; }

        public LeagueLocationDialog()
        {
            InitializeComponent();
        }

        private async void SelectLeagueLocationButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (File.Exists(string.Format(@"{0}\League of Legends.exe", dialog.FileName)))
                {
                    this.ViewModel.LeagueLocation = dialog.FileName;
                }
                else
                {
                    await DialogHelper.ShowMessageDialog("You've selected an incorrect League of Legends game folder.\n" +
                        @"Make sure it you're selecting the ""Game"" folder that contains the League of Legends.exe file" + '\n' +
                        @"For official servers this is: C:\Riot Games\League of Legends\Game" + '\n' +
                        @"For Garena: C:\Program Files (x86)\lol\{numbers}\Game");
                }
            }
        }
    }
}

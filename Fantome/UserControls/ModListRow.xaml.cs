using Fantome.MVVM.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Fantome.UserControls
{
    /// <summary>
    /// Interaction logic for ModListRow.xaml
    /// </summary>
    public partial class ModListRow : UserControl
    {
        public ModListItemViewModel ViewModel => this.DataContext as ModListItemViewModel;

        public ModListRow()
        {
            InitializeComponent();
        }

        private async void IsInstalledToggle_Checked(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Install();
        }

        private async void IsInstalledToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            await this.ViewModel.Uninstall();
        }

        private void RemoveModButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Remove();
        }
    }
}

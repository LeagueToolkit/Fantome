using Fantome.MVVM.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Fantome.MVVM.ModelViews
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

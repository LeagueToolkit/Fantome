using Fantome.MVVM.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Fantome.MVVM.ModelViews
{
    /// <summary>
    /// Interaction logic for ModCard.xaml
    /// </summary>
    public partial class ModCard : UserControl
    {
        public ModListItemViewModel ViewModel { get => this.DataContext as ModListItemViewModel; }

        public ModCard()
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

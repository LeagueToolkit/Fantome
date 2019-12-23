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
using Fantome.MVVM.ViewModels;

namespace Fantome.UserControls
{
    /// <summary>
    /// Interaction logic for ModCard.xaml
    /// </summary>
    public partial class ModCard : UserControl
    {
        public ModCardViewModel ViewModel { get => this.DataContext as ModCardViewModel; }

        public ModCard()
        {
            InitializeComponent();
        }

        private void IsInstalledToggle_Checked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Install();
        }

        private void IsInstalledToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Uninstall();
        }

        private void RemoveModButton_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Remove();
        }
    }
}

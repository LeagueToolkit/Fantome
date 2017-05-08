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
using System.Windows.Shapes;
using Fantome.Helpers;
using System.Windows.Forms;

namespace Fantome
{
    /// <summary>
    /// Interaction logic for FirstTimeWindow.xaml
    /// </summary>
    public partial class FirstTimeWindow : Window
    {
        public FirstTimeWindow()
        {
            InitializeComponent();
            this.TextboxLeaguePath.Text = Globals.GetLeaguePath();
        }

        private void ButtonLeaguePathSelect_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowDialog();
            this.TextboxLeaguePath.Text = Globals.LeaguePath = fbd.SelectedPath;
        }
    }
}

using Fantome.Helpers;
using Fantome.IO.Config;
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

namespace Fantome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FirstTimeWindow FirstTimeWindow { get; set; } = new FirstTimeWindow();
        public static ConfigFile Config { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            if(File.Exists("config.cfg"))
            {
                Config = new ConfigFile("config.cfg");
            }
            else
            {
                this.FirstTimeWindow.Show();
                Config = new ConfigFile();
                Config.AddSetting("League of Legends Path", Globals.LeaguePath);
            }
        }
    }
}

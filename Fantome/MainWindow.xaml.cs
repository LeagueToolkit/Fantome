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
using Fantome.IO;

namespace Fantome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ConfigFile config = new ConfigFile("config.json");
            //config.AddSetting("lolpath", "C:/Riot Games/Legue of Legends");
            //config.AddSetting("save skins", "True");
            //config.AddSetting("chewy is god", "False");
            config.Save("config.json");
            InitializeComponent();
        }
    }
}

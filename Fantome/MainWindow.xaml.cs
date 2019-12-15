using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public ModManager ModManager { get; private set; }

        public MainWindow()
        {
            this.ModManager = new ModManager("C:/Riot Games/League of Legends", "");

            InitializeComponent();
        }
    }
}

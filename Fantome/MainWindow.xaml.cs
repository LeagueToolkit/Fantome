using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Fantome
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CollectionViewSource.GetDefaultView(tabControl.Items).CollectionChanged += TabItemsChanged;
        }

        private void TabItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TabItem removedTab in e.OldItems)
                {
                    if (removedTab.Tag is Common.Module)
                    {
                        ((Common.Module)removedTab.Tag).Dispose();
                    }
                }
            }
        }

        private static TabItem GetTabItemFromModule(Common.Module module)
        {
            return new TabItem()
            {
                Tag = module,
                Header = module.Name,
                Content = module.GetView()
            };
        }
    }
}

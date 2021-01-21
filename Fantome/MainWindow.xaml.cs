using Fantome.ModManagement;
using Fantome.MVVM.ViewModels;
using Fantome.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using DataFormats = System.Windows.DataFormats;
using MessageBox = System.Windows.MessageBox;
using Serilog;

namespace Fantome
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindowViewModel ViewModel
        {
            get => this._viewModel;
            set
            {
                this._viewModel = value;
                NotifyPropertyChanged();
            }
        }

        private MainWindowViewModel _viewModel;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            // Hook global exception handler
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Initial checks to see if we can run Fantome
            StartupGuard.CheckEnvironment();
            StartupGuard.CheckEnvironmentPrivilage();
            StartupGuard.CheckForExistingProcess();
            StartupGuard.CheckForPathUnicodeCharacters();

            // Initialize critical components
            Config.Load();
            Logging.Initialize();
            CreateWorkFolders();

            InitializeComponent();
            BindDialogs();
            ThemeHelper.LoadTheme();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = "A Fatal Error has occurred, Fantome will now terminate.\n";
            message += "Please delete the FILE_INDEX.json, MOD_DATABSE.json files and Overlay folder if the error happened during Installation or Uninstallation.\n\n";
            message += ((Exception)e.ExceptionObject).GetType() + ": ";
            message += ((Exception)e.ExceptionObject).Message + '\n';

            Log.Fatal((Exception)e.ExceptionObject, "");
            MessageBox.Show(message, "Fantome - Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void InitializeMainViewModel()
        {
            this.ViewModel = new MainWindowViewModel();
            this.ViewModel.Initialize();
        }
        private void BindTrayIconEvents()
        {
            this.ViewModel.TrayIcon.DoubleClick += delegate (object sender, EventArgs args)
            {
                this.Topmost = true;
                Show();
                this.WindowState = WindowState.Normal;
                this.Topmost = false;
            };
        }

        private void CreateWorkFolders()
        {
            Log.Information("Creating Work folders");
            Directory.CreateDirectory(ModManager.MOD_FOLDER);
            Directory.CreateDirectory(ModManager.OVERLAY_FOLDER);
            Directory.CreateDirectory(Logging.LOGS_FOLDER);
        }
        private void BindDialogs()
        {
            Log.Information("Binding Dialogs to DialogHelper");

            DialogHelper.MessageDialog = this.MessageDialog;
            DialogHelper.OperationDialog = this.OperationDialog;
            DialogHelper.RootDialog = this.RootDialog;
        }

        private async void OnRootDialogLoad(object sender, EventArgs e)
        {
            InitializeMainViewModel();
            BindTrayIconEvents();

            this.DataContext = this.ViewModel;

            await this._viewModel.CheckForUpdate();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                if (Config.Get<bool>("MinimizeToTray"))
                {
                    this.ViewModel.TrayIcon.Visible = true;
                    Hide();
                }
            }
            else if (this.WindowState == WindowState.Normal)
            {
                this.ViewModel.TrayIcon.Visible = false;
            }

            base.OnStateChanged(e);
        }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            this.ViewModel?.TrayIcon.Dispose();
        }
        private async void OnDrop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                bool triedToImportInvalidFiles = false;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                for (int i = 0; i < files.Length; i++)
                {
                    if (Path.GetExtension(files[i]) == ".zip" || Path.GetExtension(files[i]) == ".fantome")
                    {
                        await this.ViewModel.AddMod(files[i]);
                    }
                    else
                    {
                        triedToImportInvalidFiles = true;
                    }
                }

                // Show invalid files to user
                if (triedToImportInvalidFiles)
                {
                    await DialogHelper.ShowMessageDialog("Fantome was unable to import one or more of the files you tried to import");
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

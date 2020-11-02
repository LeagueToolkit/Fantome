using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows;

namespace Fantome.Utilities
{
    public static class StartupGuard
    {
        public static void CheckEnvironment()
        {
            //First we check if Fantome is running in Wine and if it isn't then we can check windows version
            if (!WineDetector.IsRunningInWine())
            {   // AS OF VERSION 1.1, FANTOME SUPPORTS ALL WINDOWS VERSIONS
                //OperatingSystem operatingSystem = Environment.OSVersion;
                //if (operatingSystem.Version.Major != 10)
                //{
                //    MessageBox.Show("You need to be running Windows 10 in order to properly use Fantome\n"
                //        + @"By clicking the ""OK"" button you acknowledge that Fantome may not work correctly on your Windows version",
                //        "", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
        }
        public static void CheckForExistingProcess()
        {
            foreach (Process process in Process.GetProcessesByName("Fantome").Where(x => x.Id != Process.GetCurrentProcess().Id))
            {
                if (process.MainModule.ModuleName == "Fantome.exe")
                {
                    if (MessageBox.Show("There is already a running instance of Fantome.\nPlease check your tray.", "", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
        }
        public static void CheckForPathUnicodeCharacters()
        {
            string currentDirectory = Environment.CurrentDirectory;
            if (currentDirectory.Any(c => c > 255))
            {
                string message = currentDirectory + '\n';
                message += "The path to Fantome contains Unicode characters, please remove them from the path or move Fantome to a different directory\n";
                message += "Unicode characters are letters from languages such as Russian, Chinese etc....";

                if (MessageBox.Show(message, "", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        public static void CheckEnvironmentPrivilage()
        {
            try
            {
                // If any of these throw an exception then we cannot continue operating
                Directory.CreateDirectory("test");
                Directory.Delete("test");

                File.WriteAllBytes("test", new byte[0]);
                File.Delete("test");
            }
            catch(Exception exception)
            {
                string messageFormat = "Fantome does not have the required access rights to work within its folder\n{0}";
                if (MessageBox.Show(string.Format(messageFormat, exception), "", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
        }
    }
}

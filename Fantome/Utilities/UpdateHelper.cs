using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Utilities
{
    public static class UpdateHelper
    {
        public static async Task<UpdateInfo> CheckForUpdate()
        {
            try
            {
                GitHubClient gitClient = new GitHubClient(new ProductHeaderValue("Fantome"));

                IReadOnlyList<Release> releases = await gitClient.Repository.Release.GetAll("LoL-Fantome", "Fantome");
                Release newestRelease = releases[0];

                if (Version.TryParse(newestRelease.TagName, out Version newestVersion))
                {
                    if (!newestRelease.Prerelease && newestVersion > Assembly.GetExecutingAssembly().GetName().Version)
                    {
                        await DialogHelper.ShowMessageDialog("A new version of Fantome is available." + '\n' + @"Click the ""Update"" button to download it.");

                        return new UpdateInfo(true);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.Information("Unable to check for updates " + exception);
            }

            return new UpdateInfo(false);
        }
    }

    public sealed class UpdateInfo
    {
        public bool IsUpdateAvailable { get; private set; }

        public UpdateInfo(bool isUpdateAvailable)
        {
            this.IsUpdateAvailable = isUpdateAvailable;
        }
    }
}

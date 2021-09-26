using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fantome.Store.Modules.Config;
using Fluxor;
using Octokit;
using Windows.Storage;
using Windows.Web.Http;

namespace Fantome.Store.Modules.Hashtable
{
    public class FetchHashtableEffects
    {
        private readonly HttpClient _httpClient;
        private readonly IState<ConfigState> _config;

        public FetchHashtableEffects(HttpClient httpClient, IState<ConfigState> config)
        {
            this._httpClient = httpClient;
            this._config = config;
        }

        [EffectMethod]
        public async Task HandleFetchHashtableRequest(FetchHashtableAction.Request _, IDispatcher dispatcher)
        {
            GitHubClient githubClient = new(new ProductHeaderValue("Fantome"));
            IReadOnlyList<RepositoryContent> content = await githubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox");
            RepositoryContent gameHashesContent = content.FirstOrDefault(x => x.Name == "hashes.game.txt");
            RepositoryContent lcuHashesContent = content.FirstOrDefault(x => x.Name == "hashes.lcu.txt");

            await DownloadHashtable(
                new(gameHashesContent.DownloadUrl),
                this._config.Value.GameHashtablePath,
                this._config.Value.GameHashtableChecksum,
                gameHashesContent.Sha);

            await DownloadHashtable(
                new(lcuHashesContent.DownloadUrl),
                this._config.Value.LCUHashtablePath,
                this._config.Value.LCUHashtableChecksum,
                lcuHashesContent.Sha);

        }

        private async Task DownloadHashtable(Uri uri, string filePath, string currentChecksum, string newChecksum)
        {
            if (string.IsNullOrEmpty(currentChecksum) || newChecksum != currentChecksum)
            {
                string hashtable = await this._httpClient.GetStringAsync(uri);
                StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filePath, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, hashtable);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fantome.Store.Modules.Config;
using Fluxor;
using LeagueToolkit.Helpers.Cryptography;
using Octokit;
using Windows.Storage;

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
        public async Task HandleFetchHashtableRequest(FetchHashtableAction.Request action, IDispatcher dispatcher)
        {
            GitHubClient githubClient = new(new ProductHeaderValue("Fantome"));
            IReadOnlyList<RepositoryContent> content = await githubClient.Repository.Content.GetAllContents("CommunityDragon", "CDTB", "cdragontoolbox");
            RepositoryContent gameHashesContent = content.FirstOrDefault(x => x.Name is "hashes.game.txt");
            RepositoryContent lcuHashesContent = content.FirstOrDefault(x => x.Name is "hashes.lcu.txt");

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

            Dictionary<ulong, string> gameHashtable = await ParseHashtable(this._config.Value.GameHashtablePath);
            Dictionary<ulong, string> lcuHashtable = await ParseHashtable(this._config.Value.LCUHashtablePath);

            Dictionary<ulong, string> finalHashtable = gameHashtable
                .Concat(lcuHashtable.Where(x => gameHashtable.ContainsKey(x.Key) is false))
                .ToDictionary(x => x.Key, x => x.Value);

            dispatcher.Dispatch(new FetchHashtableAction.Success() { Hashtable = finalHashtable });
            dispatcher.Dispatch(new SetGameHashtableChecksumAction() { GameHashtableChecksum = gameHashesContent.Sha });
            dispatcher.Dispatch(new SetLCUHashtableChecksumAction() { LCUHashtableChecksum = gameHashesContent.Sha });
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

        private async Task<Dictionary<ulong, string>> ParseHashtable(string filePath)
        {
            Dictionary<ulong, string> hashtable = new();
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filePath);

            foreach (string line in await FileIO.ReadLinesAsync(file))
            {
                string[] lineSplit = line.Split(' ');
                ulong hash;
                string name = string.Empty;

                if (lineSplit.Length == 1)
                {
                    hash = XXHash.XXH64(Encoding.ASCII.GetBytes(lineSplit[0].ToLower()));
                    name = lineSplit[0];
                }
                else
                {
                    for (int i = 1; i < lineSplit.Length; i++)
                    {
                        name += lineSplit[i];

                        if (i + 1 != lineSplit.Length)
                        {
                            name += ' ';
                        }
                    }

                    hash = ulong.Parse(lineSplit[0], NumberStyles.HexNumber);
                }

                if (!hashtable.ContainsKey(hash))
                {
                    hashtable.Add(hash, name);
                }
            }

            return hashtable;
        }
    }

    public class Effects
    {
        [EffectMethod]
        public Task HandleFetchConfigSuccess(FetchConfigAction.Success action, IDispatcher dispatcher)
        {
            dispatcher.Dispatch(new FetchHashtableAction.Request());

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;
using Fantome.Store.Modules.Config;
using Fantome.Store.Modules.GameIndex;
using Fantome.Store.Modules.Hashtable;
using Fluxor;

namespace Fantome.Services.WadRepository
{
    public interface IWadRepositoryService
    {
        Task<WadFolder> Synchronize(GameIndexState gameIndex, HashtableState hashtable);
    }

    public class WadRepositoryService : IWadRepositoryService
    {
        public WadFolder Root { get; private set; } = new(null, string.Empty);

        public Task<WadFolder> Synchronize(GameIndexState gameIndex, HashtableState hashtable)
        {
            // Easiest way to generate the folder-file structure is to make a root folder
            // and feed it entry hashes through an Add function and letting it handle the rest
            this.Root = new(null, string.Empty);

            foreach(KeyValuePair<ulong, List<string>> entry in gameIndex.EntryToWadsMap)
            {
                string entryPath = hashtable.Get(entry.Key);

                this.Root.AddFile(entryPath);
            }

            return Task.FromResult(this.Root);
        }
    }
}

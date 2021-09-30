using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Services.WadRepository;
using Fluxor;

namespace Fantome.Store.Modules.Explorer
{
    public class Reducers
    {
        [ReducerMethod]
        public static ExplorerState HandleSynchronizeWadRepositorySuccess(ExplorerState state, SynchronizeWadRepositoryAction.Success action)
        {
            HashSet<ExplorerWadItem> explorerItems = new();
        
            foreach(WadItem wadItem in action.RootFolder.Items)
            {
                if (wadItem is WadFile wadFile)
                {
                    explorerItems.Add(ProcessWadFile(wadFile));
                }
                else if (wadItem is WadFolder wadFolder)
                {
                    explorerItems.Add(ProcessWadFolder(wadFolder));
                }
            }

            return state with { IndexedItems = explorerItems };
        }

        private static ExplorerWadItem ProcessWadFolder(WadFolder wadFolder)
        {
            HashSet<ExplorerWadItem> items = new();

            foreach (WadItem wadItem in wadFolder.Items)
            {
                if (wadItem is WadFile wadFile)
                {
                    items.Add(ProcessWadFile(wadFile));
                }
                else if (wadItem is WadFolder childWadFolder)
                {
                    items.Add(ProcessWadFolder(childWadFolder));
                }
            }

            return new() 
            {
                Name = wadFolder.Name,
                Path = wadFolder.Path,
                Items = items
            };
        }

        private static ExplorerWadItem ProcessWadFile(WadFile wadFile)
        {
            return new()
            {
                Name = wadFile.Name ?? wadFile.Path,
                Path = wadFile.Path,
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;
using LeagueToolkit.IO.WadFile;

namespace Fantome.Store.Modules.Explorer
{
    public class Effects
    {
        public async Task HandleAddWadRequest(AddWadAction.Request action, IDispatcher dispatcher)
        {
            try
            {
                using Wad wad = Wad.Mount(action.WadFileLocation, false);


            }
            catch(Exception exception)
            {
                dispatcher.Dispatch(new AddWadAction.Failure() { Error = exception});
            }
        }
    }
}

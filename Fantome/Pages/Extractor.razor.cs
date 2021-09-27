using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Store.Modules.Explorer;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fantome.Pages
{
    public partial class Extractor
    {
        [Inject] public IDispatcher Dispatcher { get; set; }

        public void OpenWadFile()
        {
            CommonOpenFileDialog commonOpenFileDialog = new();

            commonOpenFileDialog.Filters.Add(new("WAD Files", "*.wad.client;*.wad"));

            if(commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.Dispatcher.Dispatch(new AddWadAction.Request() { WadFileLocation = commonOpenFileDialog.FileName });
            }
        }
    }
}

using Fantome.Dialogs;
using Fantome.Store.Modules.App;
using Fantome.Store.Modules.Config;
using Fantome.Utilities;
using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.Maui.Essentials;
using Microsoft.WindowsAPICodePack.Dialogs;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Shared
{
    public partial class MainLayout : IComponent
    {
        [Inject] private IDialogService DialogService { get; set; }
        [Inject] private IState<ConfigState> ConfigState { get; set; }
        [Inject] public IState<AppState> AppState { get; set; }

        [Parameter] public RenderFragment Body {get; set;}

        private readonly MudTheme _theme = new()
        {
            Palette = new Palette()
            {
                Primary = "#1ddb9c",
                Black = "#27272f",
                Background = "#32333d",
                BackgroundGrey = "#27272f",
                Surface = "#373740",
                DrawerBackground = "#27272f",
                DrawerText = "rgba(255,255,255, 0.50)",
                DrawerIcon = "rgba(255,255,255, 0.50)",
                AppbarBackground = "#27272f",
                AppbarText = "rgba(255,255,255, 0.70)",
                TextPrimary = "rgba(255,255,255, 0.70)",
                TextSecondary = "rgba(255,255,255, 0.50)",
                ActionDefault = "#adadb1",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                TableLines = "rgba(255,255,255, 0.12)",
                LinesDefault = "rgba(255,255,255, 0.12)",
                LinesInputs = "rgba(255,255,255, 0.3)",
                TextDisabled = "rgba(255,255,255, 0.2)"
            }
        };

        private bool _isNavDrawerOpen = true;

        private void ToggleNavDrawer() => this._isNavDrawerOpen = !this._isNavDrawerOpen;
    }
}

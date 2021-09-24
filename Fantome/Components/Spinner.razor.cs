using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Fantome.Components
{
    public partial class Spinner
    {
        [Parameter] public string Message { get; set; }
    }
}

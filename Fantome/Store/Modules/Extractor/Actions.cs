using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Extractor
{
    public class AddWadAction
    {
        public string WadFileLocation { get; set; }
    }
    public class AddWadActionSuccess
    {

    }
    public class AddWadActionError
    {
        public Exception Error { get; set; }
    }
}

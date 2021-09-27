using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Explorer
{
    public class AddWadAction
    {
        public class Request : AsyncActionRequest
        {
            public string WadFileLocation { get; set; }
        }
        public class Success : AsyncActionSuccess { }
        public class Failure : AsyncActionFailure
        {
            public Exception Error { get; set; }
        }
    }

    public class SynchronizeWadRepositoryAction
    {
        public class Request : AsyncActionRequest { }
        public class Success : AsyncActionSuccess { }
        public class Failure : AsyncActionFailure
        {
            public Exception Error { get; set; }
        }
    }
}

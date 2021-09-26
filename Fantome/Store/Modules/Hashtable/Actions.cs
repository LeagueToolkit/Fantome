using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Hashtable
{
    public class FetchHashtableAction
    {
        public class Request : AsyncActionRequest { }
        public class Success : AsyncActionSuccess 
        {
            public Dictionary<ulong, string> Hashtable { get; set; }
        }
        public class Failure : AsyncActionFailure 
        {
            public Exception Error { get; set; }
        }
    }

    public class ExtendHashtableAction
    {
        public Dictionary<ulong, string> Hashtable { get; set; }
    }
}

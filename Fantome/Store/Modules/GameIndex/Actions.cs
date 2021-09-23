using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fantome.Core;

namespace Fantome.Store.Modules.GameIndex
{
    public class BuildGameIndexAction
    {
        public class Request : AsyncActionRequest { }
        public class Success : AsyncActionSuccess 
        {
            public GameIndexStorage GameIndex { get; set; }
        }
        public class Failure : AsyncActionFailure 
        { 
            public Exception Error { get; set; } 
        }
    }
}

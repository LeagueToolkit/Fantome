﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Extractor
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
}
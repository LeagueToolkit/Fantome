using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store
{
    public interface ConfigAction { }
    
    public interface AsyncAction { }
    public interface AsyncActionRequest : AsyncAction { }
    public interface AsyncActionSuccess : AsyncAction { }
    public interface AsyncActionFailure : AsyncAction
    {
        Exception Error { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluxor;

namespace Fantome.Store.Modules.Hashtable
{
    public class Reducers
    {
        [ReducerMethod]
        public static HashtableState HandleFetchHashtableSuccess(HashtableState state, FetchHashtableAction.Success action)
        {
            return new(action.Hashtable);
        }

        [ReducerMethod]
        public static HashtableState HandleExtendHashtable(HashtableState state, ExtendHashtableAction action)
        {
            Dictionary<ulong, string> extendedHashtable = new(state.GetTable());

            foreach (KeyValuePair<ulong, string> extendEntry in action.Hashtable)
            {
                if (!extendedHashtable.ContainsKey(extendEntry.Key))
                {
                    extendedHashtable.Add(extendEntry.Key, extendEntry.Value);
                }
            }

            return new(extendedHashtable);
        }
    }
}

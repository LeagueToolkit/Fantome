using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.Store.Modules.Hashtable
{
    public record HashtableState
    {
        private readonly Dictionary<ulong, string> _table = new();

        public HashtableState(Dictionary<ulong, string> table)
        {
            this._table = table;
        }

        public string Get(ulong key)
        {
            if (this._table.ContainsKey(key))
            {
                return this._table[key];
            }
            else
            {
                return key.ToString("x16");
            }
        }

        public ReadOnlyDictionary<ulong, string> GetTable() => new(this._table);
    }
}

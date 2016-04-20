using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Script
{
    public class ICASMExecutionResult
    {
        private Dictionary<string, object> _data;
        public Dictionary<string, object> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private bool _success;
        public bool Success { get { return _success; } set { _success = value; } }
        
        public ICASMExecutionResult() { _data = new Dictionary<string, object>(); }

        public override string ToString()
        {
            string s = "";
            foreach(KeyValuePair<string, object> d in Data)
            {
                s += "\n\t" + d.Key + " = " + d.Value;
            }
            return _success + " - " + s;
        }
    }
}

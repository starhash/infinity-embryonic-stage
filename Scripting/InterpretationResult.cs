using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting
{
    public class InterpretationResult
    {
        private object _result;
        public object Result { get { return _result; } set { _result = value; } }
        private Dictionary<string, object> _data;
        public Dictionary<string, object> Data { get { return _data; } }

        public InterpretationResult()
        {
            _data = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                return _data[key];
            }
        }
    }
}

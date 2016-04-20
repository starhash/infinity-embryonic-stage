using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Infinity.Scripting.Utils
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TestResult<R>
    {
        private R _result;
        public R Result { get { return _result; } set { _result = value; } }
        private TestResultType _type;
        public TestResultType Type { get { return _type; } set { _type = value; } }
        private Dictionary<string, object> _data;
        public Dictionary<string, object> Data
        {
            get { return _data; }
        }

        public TestResult(R result, TestResultType type)
        {
            Result = result;
            Type = type;
            _data = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            string data = "";
            foreach (string key in Data.Keys)
            {
                data += "\n\t" + key + "\t:\n\t\t" + Data[key] + ", ";
            }
            return Result + ", " + Type + " : " + data;
        }

        public object this[string key]
        {
            get
            {
                return Data[key];
            }
        }
    }
}

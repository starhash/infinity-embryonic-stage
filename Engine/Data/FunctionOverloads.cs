using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data
{
    public class FunctionOverloads
    {
        private List<Function> _overloads;
        private string _function_access_key;
        public List<Function> Overloads { get { return _overloads; } }
        public string AccessKey { get { return _function_access_key; } }

        public FunctionOverloads(string accesskey)
        {
            _function_access_key = accesskey;
            _overloads = new List<Function>();
        }

        public bool AddFunction(Function function)
        {
            bool foundmatch = false;
            foreach (Function f in _overloads)
            {
                if (Function.MatchesParameters(function, f))
                {
                    foundmatch = true;
                    break;
                }
            }
            if (!foundmatch)
            {
                _overloads.Add(function);
                return foundmatch;
            }
            return foundmatch;
        }

        public Function GetMatch(params Address[] _parameters)
        {
            foreach (Function f in _overloads)
            {
                if (Function.ParametersMatch(f, _parameters))
                {
                    return f;
                }
            }
            return null;
        }
    }
}

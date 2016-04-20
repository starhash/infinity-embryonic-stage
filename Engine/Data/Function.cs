using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Engine.Script;

namespace Infinity.Engine.Data
{
    public class Function : Addressable
    {
        private string _function_name;
        private string _function_access_keyword;
        private FunctionType _function_type;
        private List<Address> _function_parameters;
        private bool _single_repetitive_parameterized;

        public string Name { get { return _function_name; } }
        public string AccessKeyword { get { return _function_access_keyword; } }
        public FunctionType Type { get { return _function_type; } }
        public List<Address> Parameters { get { return _function_parameters; } }
        public bool SingleRepetitiveParameterized { get { return _single_repetitive_parameterized; } set { _single_repetitive_parameterized = value; } }

        public delegate Variable Executor(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses);
        public event Executor ExecuteFunction;

        public Function(string name, string accesskeyword, FunctionType type, Address parent, params Address[] _parameters)
        {
            _function_name = name;
            _function_access_keyword = accesskeyword;
            _function_type = type;
            _address = new Address(parent.Path + "/" + parent.Name, name, AddressType.Function);
            _function_parameters = _parameters.ToList();
        }

        public Variable Execute(Address _destination_address, params Address[] _parameters)
        {
            Variable[] vars = new Variable[_parameters.Length];
            vars[0] = RuntimeEngine.GetVariable(_parameters[0]);
            bool matching = true;
            int i = 1;
            for (; i < _parameters.Length; i++)
            {
                vars[i] = RuntimeEngine.GetVariable(_parameters[i]);
                int idx = i;
                if (_single_repetitive_parameterized)
                    idx = 0;
                if (vars[i].Type.HasCastTo(Parameters[idx]))
                {
                    Address _tempAddress = new Address(_parameters[i].Path, "$SYSTEM$_temp"+ICASMInterpreter.TemporaryVariableCount, AddressType.Variable);
                    Variable _tempCast = vars[i].Type.ExecuteFunction(_tempAddress, "$SYSTEM$_Runtime.CastTo_" + Parameters[idx].FullPath.Replace("/", "_").Replace("/$SYSTEM$_Runtime.TypeSpace@TypeEngine", ""), _parameters[i]);
                    _parameters[i] = _tempAddress;
                    ICASMInterpreter.MemoryStack.Push(_tempAddress);
                    ICASMInterpreter.TemporaryVariableCount++;
                    vars[i] = _tempCast;
                }
                matching = matching && (vars[i].Type.Address.Equals(Parameters[idx]));
                if (!matching) break;
            }
            if (i == _parameters.Length)
            {
                return ExecuteFunction(this, _destination_address, _parameters);
            }
            else
            {
                return null;
            }
        }

        public bool Equals(Function function)
        {
            bool equivalence = _function_name.Equals(function._function_name);
            if (equivalence)
            {
                equivalence = this.Address.ToString().Equals(function.Address.ToString());
                if (equivalence)
                {
                    equivalence = _function_access_keyword.Equals(function._function_access_keyword);
                    if (equivalence)
                    {
                        equivalence = _function_type.Equals(function._function_type);
                        if (equivalence)
                        {
                            equivalence = MatchesParameters(this, function);
                            return equivalence;
                        }
                    }
                }
            }
            return false;
        }

        public static bool MatchesParameters(Function f1, Function f2)
        {
            bool equivalence = f1._function_parameters.Count == f2._function_parameters.Count;
            if (equivalence)
            {
                if (f1._function_parameters.Count == 0) return true;
                equivalence = true;
                for (int i = 0; i < f1._function_parameters.Count && equivalence; i++)
                {
                    equivalence = equivalence && (f1._function_parameters[i].Equals(f2._function_parameters[i]) || TypeEngine.GetType(f1._function_parameters[i]).HasCastTo(f2._function_parameters[i]));
                }
                return equivalence;
            }
            return false;
        }

        public static bool ParametersMatch(Function function, params Address[] _parameters)
        {
            bool equivalence = function._function_parameters.Count == _parameters.Length;
            if (function.SingleRepetitiveParameterized) equivalence = true;
            if (equivalence)
            {
                if (_parameters.Length == 0) return true;
                equivalence = true;
                for (int i = 1; i < _parameters.Length && equivalence; i++)
                {
                    Variable temp = RuntimeEngine.GetVariable(_parameters[i]);
                    if (!function.SingleRepetitiveParameterized)
                    {
                        bool funcequal = function.Parameters[i].Equals(temp.Type.Address);
                        bool hascastto = temp.Type.HasCastTo(function.Parameters[i]);
                        equivalence = equivalence && (funcequal || hascastto);
                    }
                    else
                    {
                        bool funcequal = function.Parameters[0].Equals(temp.Type.Address);
                        /*if (temp.Type.Address.FullPath.Equals("/Object"))
                        {
                            Variable temp_var = new Variable(temp.Value, )
                        }*/
                        bool hascastto = temp.Type.HasCastTo(function.Parameters[0]);
                        equivalence = equivalence && (funcequal || hascastto);
                    }
                }
                return equivalence;
            }
            return false;
        }
    }
}

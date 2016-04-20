using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Engine.Data;
using Infinity.Scripting;

namespace Infinity.Engine.Script
{
    public class ICASMFunction : Function
    {
        private Queue<string> _executionQueue;
        public Queue<string> ExecutionQueue { get { return _executionQueue; } }

        public ICASMFunction(string functionindentifier, string name, FunctionType type, Address parent, params Address[] parametertypes)
            : base(name, functionindentifier, type, parent, parametertypes)
        {
            _executionQueue = new Queue<string>();
            ExecuteFunction += ICASMFunction_ExecuteFunction;
        }

        private Variable ICASMFunction_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            ICASMFunction func = (ICASMFunction)_executing_function;
            string[] statements = func.ExecutionQueue.ToArray();
            for(int i = 0; i<statements.Length; i++)
            {
                for(int j = 0; j<_parameter_addresses.Length; j++)
                {
                    statements[i] = statements[i].Replace("[" + j + "]", _parameter_addresses[j].FullPath);
                }
            }
            Scope _functionScope = new Scope("/" + func.Name);
            ICASMExecutionResult result = ICASMInterpreter.Execute(_functionScope, statements);
            Variable result_variable = null;
            if (result.Data.ContainsKey("ReturnDirective"))
            {
                result_variable = RuntimeEngine.GetVariable((Address)result.Data["ReturnDirective"]);
                RuntimeEngine.PutVariable(_destination_address, result_variable);
            }
            return result_variable;
        }
    }
}

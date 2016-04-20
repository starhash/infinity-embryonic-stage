using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Infinity.Scripting;
using Infinity.Scripting.Grammar;
using Infinity.Scripting.Utils;
using Infinity.Scripting.Grammar.Parsing;
using Infinity.Engine;
using Infinity.Engine.Data;
using Infinity.Engine.Data.DefaultTypes;
using Infinity.Engine.Script;

namespace Run
{
    class Program
    {
        static void Main(string[] args)
        {
            VariablePool pool = RuntimeEngine.VariablePool;
            TypeSpace space = TypeEngine.RuntimeTypes;
            ICASMInterpreter.LoadAndExecute(@"G:\test2.icasm");
            var stack = ICASMInterpreter.MemoryStack;
            Console.ReadKey();
            
            return;
        }
    }
}

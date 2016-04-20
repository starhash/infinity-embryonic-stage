using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Infinity.Scripting;
using Infinity.Engine;
using Infinity.Engine.Data;

namespace icon
{
    public class InfinityInterpreter : Interpreter
    {
        public override InterpretationResult Interpret(System.Xml.XmlElement _node, Scope _scope)
        {
            Infinity_Grammar_Script_1_0 type = (Infinity_Grammar_Script_1_0)Enum.Parse(typeof(Infinity_Grammar_Script_1_0), _node.Name);
            Variable value = null;
            InterpretationResult result = new InterpretationResult();
            switch (type)
            {
                
            }
            return null;
        }
    }
}

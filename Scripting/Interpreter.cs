using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Infinity.Scripting
{
    public abstract class Interpreter
    {
        public abstract InterpretationResult Interpret(XmlElement _node, Scope _scope);
    }
}

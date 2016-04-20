using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting
{
    public class GrammarScriptItem
    {
        private GrammarScriptItemType _type;
        public GrammarScriptItemType Type { get { return _type; } set { _type = value; } }

        private string _name;
        public string Name { get { return _name; } set { _name = value; } }

        public GrammarScriptItem(GrammarScriptItemType type, string name)
        {
            _name = name;
            _type = type;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Infinity.Scripting.Grammar.Parsing
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ParseTreeNode : TreeNode<string>
    {
        protected string _nodename;
        public string Name { get { return _nodename; } set { _nodename = value; } }

        public ParseTreeNode()
        {
            _children = new List<TreeNode<string>>();
        }

        public string ToTextualRepresentation()
        {
            string s = "";
            s += Name + ((Value == null) ? "" : (" = " + Value));
            for (int i = 0; i < Children.Count; i++)
            {
                ParseTreeNode p = (ParseTreeNode)Children[i];
                s += "\n" + p.ToTextualRepresentation().Replace("\n", "\n    ");
            }
            return s;
        }

        public string ToXMLTextRepresentation()
        {
            string s = "";
            string _formattedName = Name;
            string quoteValue = "#quote;";
            _formattedName = _formattedName.Replace("*", "_Star");
            _formattedName = _formattedName.Replace("+", "_Plus");
            if (Children.Count == 0)
            {
                return "\n<" + _formattedName + ((Value == null) ? "" : (" Value = \"" + Value.Replace("\"", quoteValue)) + "\"") + " />";
            }
            bool allhavevalues = true;
            string value = "";
            foreach (ParseTreeNode p in Children)
            {
                allhavevalues = allhavevalues && (p.Value != null);
                if (allhavevalues)
                    value += p.Value + " ";
                else
                    break;
            }
            if (allhavevalues)
            {
                Value = value.Trim().Replace("\"", quoteValue);
                s += "\n<" + _formattedName + ((Value == null) ? "" : (" Value = \"" + Value.Replace("\"", quoteValue)) + "\"") + " />";
            }
            else
            {
                s += "\n<" + _formattedName + ((Value == null) ? "" : (" Value = \"" + Value.Replace("\"", quoteValue)) + "\"") + ">";
                for (int i = 0; i < Children.Count; i++)
                {
                    ParseTreeNode p = (ParseTreeNode)Children[i];
                    s += p.ToXMLTextRepresentation().Replace("\n", "\n\t");
                }
                s += "\n</" + _formattedName + ">";
            }
            return s;
        }

        public string ToCompiledParseTextRepresentation(GrammarTerminalMap map, int mappingFactor)
        {
            string cpt = "";
            int total = map.Count;
            string name = Name;
            int _formattedName = -1;
            if (name.Contains(":"))
            {
                string terminal = name.Substring(0, name.IndexOf(":"));
                string v = name.Substring(name.IndexOf(":") + 1);
                int val = int.Parse(v);
                _formattedName = map[terminal] * (val + 1) * mappingFactor;
            }
            else
            {
                _formattedName = map[name] * mappingFactor;
            }
            string quoteValue = "#quote;";
            if (Children.Count == 0)
            {
                return _formattedName + ((Value == null) ? "" : (" #" + Value.Replace("\"", quoteValue) + " " + (_formattedName + 1))) + " ";
            }
            bool allhavevalues = true;
            string value = "";
            foreach (ParseTreeNode p in Children)
            {
                allhavevalues = allhavevalues && (p.Value != null);
                if (allhavevalues)
                    value += p.Value + " ";
                else
                    break;
            }
            if (allhavevalues)
            {
                Value = value.Trim().Replace("\"", quoteValue);
                cpt += _formattedName + ((Value == null) ? "" : (" #" + Value.Replace("\"", quoteValue) + " " + (_formattedName + 1))) + " ";
            }
            else
            {
                cpt += _formattedName + ((Value == null) ? "" : (" #" + Value.Replace("\"", quoteValue) + " " + (_formattedName + 1))) + " ";
                for (int i = 0; i < Children.Count; i++)
                {
                    ParseTreeNode p = (ParseTreeNode)Children[i];
                    cpt += p.ToCompiledParseTextRepresentation(map, mappingFactor) + " ";
                }
                cpt += (_formattedName + 1);
            }
            return cpt;
        }
    }
}

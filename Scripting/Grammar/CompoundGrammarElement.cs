using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Utils;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using Infinity.Scripting.Grammar.Parsing;

namespace Infinity.Scripting.Grammar
{
    public class CompoundGrammarElement : GrammarElement
    {
        protected List<GrammarElement> _te;
        private string _delimiters;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The terminals which form the Compound grammar")]
        [Category("General")]
        [DisplayName("Terminals")]
        [Editor(typeof(GrammarElementCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public List<GrammarElement> Terminals
        {
            get
            {
                return _te;
            }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The delimiters to be used")]
        [Category("General")]
        [DisplayName("Delimiters")]
        public string Delimiters
        {
            get { return _delimiters; }
            set { _delimiters = value.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t"); }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public override SymbolSetCollection SetItems
        {
            get { return null; }
            set { Sets = null; }
        }

        public CompoundGrammarElement(params GrammarElement[] Terminals)
        {
            _te = new List<GrammarElement>();
            Delimiters = " \n\t\r";
            foreach (GrammarElement ge in Terminals)
            {
                _te.Add(ge);
            }
        }

        public void AddTerminal(GrammarElement ge)
        {
            _te.Add(ge);
        }
        public override void AddElement(SymbolSet set)
        {
            throw new InvalidOperationException("A compound grammar element cannot have SymbolSets.");
        }
        public override TestResult<bool> Validate(ref string input, bool consume)
        {
            TestResult<bool> result = new TestResult<bool>(false, TestResultType.Failed);
            string temp = input;
            if (!_delimiters.Contains(" "))
                temp = temp.Replace(" ", "[space]");
            bool check = true;
            ParseTreeNode leaf = new ParseTreeNode();
            if (this.Name == null || this.Name.Length == 0)
                leaf.Name = "__SYSTEM_GRAMMAR_CMPGE";
            else
                leaf.Name = this.Name;
            foreach (GrammarElement g in Terminals)
            {
                temp = temp.Trim(_delimiters.ToArray()).Replace("[space]", " ");
                TestResult<bool> terminalcheck = g.Validate(ref temp, true);
                check = check && terminalcheck.Result;
                if (terminalcheck.Result == false) break;
                if(g.Name != null)
                    if (g.Name.Equals("SemiColon")) 
                        Console.Write("");
                if (terminalcheck.Data.ContainsKey("$PARSETREE.NODE$") && !g.IsInterim)
                {
                    leaf.Children.Add((ParseTreeNode)terminalcheck.Data["$PARSETREE.NODE$"]);
                }
                if (!_delimiters.Contains(" "))
                    temp = temp.Replace(" ", "[space]");
            }
            if (check)
            {
                if (consume)
                    input = temp.Trim(_delimiters.ToArray()).Replace("[space]", " ");
                result.Result = true;
                result.Type = TestResultType.Complete;
                GrammarPath gpa = new GrammarPath();
                gpa.Put(this.Name);
                result.Data.Add("PATH", gpa);
                result.Data.Add("$PARSETREE.NODE$", leaf);
            }
            return result;
        }
    }
}

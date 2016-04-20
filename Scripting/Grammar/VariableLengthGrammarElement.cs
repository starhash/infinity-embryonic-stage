using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Infinity.Scripting.Utils;
using Infinity.Scripting.Grammar.Parsing;

namespace Infinity.Scripting.Grammar
{
    public class VariableLengthGrammarElement : GrammarElement
    {
        private VariableLengthGrammarElementType _type;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("Type of variable length grammar element")]
        [Category("General")]
        [DisplayName("Type")]
        public VariableLengthGrammarElementType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public VariableLengthGrammarElement()
        {
            Sets = new SymbolSetCollection();
        }
        public VariableLengthGrammarElement(VariableLengthGrammarElementType type, SymbolSet set)
        {
            Sets = new SymbolSetCollection() { Capacity = 1 };
            _type = type;
            AddElement(set);
        }
        public VariableLengthGrammarElement(VariableLengthGrammarElementType type, params SymbolSet[] sets)
        {
            Sets.AddRange(sets);
            _type = type;
        }

        public override TestResult<bool> Validate(ref string input, bool consume)
        {
            string temp = input;
            if (_type == VariableLengthGrammarElementType.Star && input.Length == 0)
            {
                TestResult<bool> res = new TestResult<bool>(true, TestResultType.Complete);
                GrammarPath gp = new GrammarPath();
                gp.Put(this.Name);
                res.Data.Add("PATH", gp);
                ParseTreeNode leaf = new ParseTreeNode();
                if (this.Name == null || this.Name.Length == 0)
                    leaf.Name = "__SYSTEM_GRAMMAR_VARGE";
                else
                    leaf.Name = this.Name;
                leaf.Value = "";
                res.Data.Add("$PARSETREE.NODE$", leaf);
                return res;
            }
            TestResult<bool> result = Sets.SatisfiedBy(ref input, consume, true);
            if (result.Result == false && _type == VariableLengthGrammarElementType.Star)
            {
                result.Result = true;
                result.Type = TestResultType.Failed;
                GrammarPath gp = new GrammarPath();
                gp.Put(this.Name);
                result.Data.Add("PATH", gp);
                return result;
            }
            if (temp.Length == 0)
            {
                if (input.Length != 0)
                {
                    result.Type = TestResultType.Complete;
                    result.Result = true;
                    result.Data.Clear();
                }
                else if (_type == VariableLengthGrammarElementType.Plus)
                {
                    result.Type = TestResultType.Failed;
                    result.Result = false;
                    result.Data.Clear();
                }
            }
            else if (temp.Length == input.Length)
            {
                result.Type = TestResultType.Failed;
                result.Result = false;
                result.Data.Clear();
            }
            else
            {
                result.Type = TestResultType.Partial;
                result.Result = true;
                result.Data.Clear();
            }
            if (!consume && result.Result)
                input = temp;
            if (result.Result)
            {
                GrammarPath gp = new GrammarPath();
                gp.Put(this.Name);
                result.Data.Add("PATH", gp);
                if (input.Length != 0)
                {
                    int indx = temp.Length - input.Length;
                    temp = temp.Remove(indx).Trim();
                } 
                ParseTreeNode leaf = new ParseTreeNode();
                if (this.Name == null || this.Name.Length == 0)
                    leaf.Name = "$SYSTEM.STRING$";
                else
                    leaf.Name = this.Name;
                leaf.Value = temp;
                result.Data.Add("$PARSETREE.NODE$", leaf);
                return result;
            }
            return new TestResult<bool>(false, TestResultType.Failed);
        }
    }
}

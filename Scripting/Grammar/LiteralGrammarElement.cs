using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Utils;
using Infinity.Scripting.Grammar.Parsing;

namespace Infinity.Scripting.Grammar
{
    public class LiteralGrammarElement : GrammarElement
    {
        public LiteralGrammarElement()
        {
            Sets = new SymbolSetCollection();
        }

        public LiteralGrammarElement(string value)
        {
            base.Sets = new SymbolSetCollection() { Capacity = 1 };
            SymbolSet uniset = new SymbolSet();
            uniset.Add(new Symbol(value));
            AddElement(uniset);
        }

        public LiteralGrammarElement(params string[] values)
        {
            SymbolSet uniset = new SymbolSet();
            foreach (string value in values)
            {
                uniset.Add(new Symbol(value));
            }
            AddElement(uniset);
        }

        public override TestResult<bool> Validate(ref string input, bool consume)
        {
            string backup = input;
            string tempinput = input;
            TestResult<bool> result = Sets.SatisfiedBy(ref input, consume, false);
            if (input.Length != 0)
            {
                int indx = tempinput.Length - input.Length;
                tempinput = tempinput.Remove(indx).Trim();
            }
            if (result.Result)
            {
                ParseTreeNode leaf = new ParseTreeNode();
                if (this.Name == null || this.Name.Length == 0)
                    leaf.Name = "__SYSTEM_GRAMMAR_LGE";
                else
                    leaf.Name = this.Name;
                leaf.Value = tempinput;
                result.Data.Add("$PARSETREE.NODE$", leaf);
                if (!consume) input = backup;
                return result;
            }
            if (!consume) input = backup;
            return new TestResult<bool>(false, TestResultType.Failed);
        }
        public static implicit operator LiteralGrammarElement(string str)
        {
            LiteralGrammarElement lge = new LiteralGrammarElement(str);
            return lge;
        }
        public static implicit operator LiteralGrammarElement(string[] strs)
        {
            LiteralGrammarElement lge = new LiteralGrammarElement(strs);
            return lge;
        }
    }
}

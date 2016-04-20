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
    public class RepetitiveGrammarElement : GrammarElement
    {
        private GrammarElement _torepeat;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The Grammar element to test for repetition")]
        [Category("General")]
        [DisplayName("Repetitive")]        
        public GrammarElement Repetitive
        {
            get { return _torepeat; }
            set { _torepeat = value; }
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public override SymbolSetCollection SetItems
        {
            get { return null; }
            set { Sets = null; }
        }

        public RepetitiveGrammarElement() { }
        public RepetitiveGrammarElement(GrammarElement ge)
        {
            _torepeat = ge;
        }

        public override Utils.TestResult<bool> Validate(ref string input, bool consume)
        {
            string backup = input;
            TestResult<bool> result = new TestResult<bool>(false, TestResultType.Failed);
            int count = 0;
            ParseTreeNode root = new ParseTreeNode();
            if (this.Name == null || this.Name.Length == 0)
                root.Name = "__SYSTEM_GRAMMAR_REPGE";
            else
                root.Name = this.Name;
            if (_torepeat == null)
                throw new InvalidOperationException("Repetitive not specified.");
            TestResult<bool> testres = null;
            while ((testres = _torepeat.Validate(ref input, true)).Result)
            {
                if (testres.Data.ContainsKey("$PARSETREE.NODE$"))
                {
                    root.Children.Add((ParseTreeNode)testres.Data["$PARSETREE.NODE$"]);
                }
                count++;
            }
            if (backup.Length != input.Length)
            {
                result.Result = true;
                result.Type = TestResultType.Complete;
                result.Data.Add("RepetitiveGrammarElement.Repetitions", count);
            }
            else
            {
                result.Result = true;
                result.Type = TestResultType.Partial;
                result.Data.Add("RepetitiveGrammarElement.Repetitions", 0);
            }
            if (result.Result)
            {
                result.Data.Add("$PARSETREE.NODE$", root);
            }
            return result;
        }
    }
}

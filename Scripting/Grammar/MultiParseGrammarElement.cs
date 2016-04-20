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
    public class MultiParseGrammarElement : CompoundGrammarElement
    {
        private MultiParsePrecedenceType _precedence;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The precedence order of parsed terminals")]
        [Category("General")]
        [DisplayName("Precedence")]
        public MultiParsePrecedenceType Precedence { get { return _precedence; } set { _precedence = value; } }

        [Browsable(false)]
        [ReadOnly(true)]
        public override SymbolSetCollection SetItems
        {
            get { return null; }
            set { Sets = null; }
        }

        public MultiParseGrammarElement() { _precedence = MultiParsePrecedenceType.Normal; }
        public MultiParseGrammarElement(MultiParsePrecedenceType precedence)
        {
            _precedence = precedence;
        }

        public override TestResult<bool> Validate(ref string input, bool consume)
        {
            List<TestResult<bool>> matchindices = new List<TestResult<bool>>();
            string temp = input;
            ParseTreeNode root = new ParseTreeNode();
            if (this.Name == null || this.Name.Length == 0)
                root.Name = "__SYSTEM_GRAMMAR_MPGE";
            else
                root.Name = this.Name;
            for (int i = 0; i < _te.Count; i++)
            {
                temp = input;
                TestResult<bool> result = _te[i].Validate(ref temp, true);
                if (result.Result)
                {
                    if (result.Data.ContainsKey("$PARSETREE.NODE$"))
                    {
                        root.Children.Add((ParseTreeNode)result.Data["$PARSETREE.NODE$"]);
                    }
                    if (!result.Data.ContainsKey("$INTERNALRESULT$"))
                        result.Data.Add("$INTERNALRESULT$", temp);
                    if(!_te[i].IsInterim)
                        matchindices.Add(result);
                }
            }
            if (matchindices.Count == 1)
            {
                TestResult<bool> result = new TestResult<bool>(true, TestResultType.Complete);
                GrammarPath gpa = null;
                if (matchindices[0].Data.ContainsKey("PATH"))
                    gpa = (GrammarPath)matchindices[0].Data["PATH"];
                else
                    gpa = new GrammarPath();
                gpa.Put(this.Name);
                result.Data.Add("PATH", gpa);
                if (matchindices[0].Data.ContainsKey("$INTERNALRESULT$"))
                    input = (string)matchindices[0].Data["$INTERNALRESULT$"];
                else
                    input = temp;
                result.Data.Add("$PARSETREE.NODE$", root);
                return result;
            }
            else if (matchindices.Count > 1)
            {
                TestResult<bool> result = new TestResult<bool>(true, TestResultType.Multiple);
                result.Data.Add("TerminalIndices", matchindices);
                if (_precedence == MultiParsePrecedenceType.Normal)
                {
                    List<string> sslist = new List<string>();
                    foreach (TestResult<bool> t in matchindices)
                    {
                        if (t.Data.ContainsKey("$INTERNALRESULT$"))
                        {
                            sslist.Add((string)t.Data["$INTERNALRESULT$"]);
                        }
                    }
                    int min = int.MaxValue;
                    int idxmin = -1;
                    for (int i = 0; i < sslist.Count; i++)
                    {
                        if (sslist[i].Length < min)
                        {
                            idxmin = i;
                            min = sslist[i].Length;
                        }
                    }
                    if (idxmin != -1)
                        input = sslist[idxmin];
                    else
                        input = temp;
                }
                else if (_precedence == MultiParsePrecedenceType.Ascending || _precedence == MultiParsePrecedenceType.Descending)
                {
                    int idx = (_precedence == MultiParsePrecedenceType.Ascending) ? 0 : matchindices.Count - 1;
                    result = new TestResult<bool>(true, TestResultType.Complete);
                    GrammarPath gpa = null;
                    if (matchindices[idx].Data.ContainsKey("PATH"))
                        gpa = (GrammarPath)matchindices[idx].Data["PATH"];
                    else
                        gpa = new GrammarPath();
                    gpa.Put(this.Name);
                    result.Data.Add("PATH", gpa);
                    if (matchindices[idx].Data.ContainsKey("$INTERNALRESULT$"))
                        input = (string)matchindices[idx].Data["$INTERNALRESULT$"];
                    else
                        input = temp;
                    TreeNode<string> satisfied = root.Children[idx];
                    root.Children.Clear();
                    root.Children.Add(satisfied);
                    result.Data.Add("$PARSETREE.NODE$", root);
                    return result;
                }
                result.Data.Add("$PARSETREE.NODE$", root);
                return result;
            }
            return new TestResult<bool>(false, TestResultType.Failed);
        }
    }
}

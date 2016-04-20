using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    public class SymbolSetCollection : List<SymbolSet>
    {
        public TestResult<bool> SatisfiedBy(ref string input, bool consume, bool repeat)
        {
            TestResult<bool> result = new TestResult<bool>(false, TestResultType.Failed);
            string temp = input;
            do
            {
                bool testall = false;
                foreach (SymbolSet ss in this)
                {
                    TestResult<bool> testres = ss.IsSatisfiedBy(temp);
                    testall = testall || testres.Result;
                    if (testres.Result)
                    {
                        string sym = (string)testres["Symbol"];
                        temp = temp.Substring(temp.IndexOf(sym) + sym.Length);
                        if(!result.Data.ContainsKey("Symbol"))result.Data.Add("Symbol", sym);
                        break;
                    }
                }
                if (!testall) break;
            } while (repeat && temp.Length != 0);
            if (temp.Length != input.Length)
            {
                if (consume) input = temp;
                result.Result = true;
                if (temp.Length == 0)
                    result.Type = TestResultType.Complete;
                else
                    result.Type = TestResultType.Partial;
            }
            return result;
        }
    }
}

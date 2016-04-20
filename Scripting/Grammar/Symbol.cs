using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Symbol
    {
        private string _symbolvalue;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The symbol value.")]
        [Category("General")]
        [DisplayName("Value")]
        public virtual string Value
        {
            get { return _symbolvalue; }
            set { _symbolvalue = value; }
        }

        public Symbol() { _symbolvalue = ""; }
        public Symbol(string symbol) { _symbolvalue = symbol; }
        public Symbol(char symbol) { _symbolvalue = symbol + ""; }

        public virtual TestResult<bool> Matches(string input)
        {
            if (_symbolvalue.Equals(input))
            {
                TestResult<bool> result = new TestResult<bool>(true, TestResultType.Complete);
                result.Data.Add("Symbol", _symbolvalue);
                return result;
            }
            else if (input.StartsWith(_symbolvalue))
            {
                TestResult<bool> result = new TestResult<bool>(true, TestResultType.Partial);
                result.Data.Add("Symbol", _symbolvalue);
                return result;
            }
            else
            {
                TestResult<bool> result = new TestResult<bool>(false, TestResultType.Failed);
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Symbol))
                return ((Symbol)obj)._symbolvalue.Equals(_symbolvalue);
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    public class SymbolRange : Symbol
    {
        private string _from;
        private string _to;

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The string lexicographically denoting the start value for the range.")]
        [Category("Range")]
        [DisplayName("From")]
        public string From
        {
            get { return _from; }
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Range start value cannot be null or empty :\n\r\t" + value + ".");
                if (To != null)
                {
                    if (value.CompareTo(To) > 0) throw new ArgumentException("Given range [" + value + " - " + To + "] is not valid.");
                }
                _from = value;
            }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The string lexicographically denoting the end value for the range.")]
        [Category("Range")]
        [DisplayName("To")]
        public string To
        {
            get { return _to; }
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentException("Range start value cannot be null or empty :\n\r\t" + value + ".");
                if (From != null)
                {
                    if (value.CompareTo(From) < 0) throw new ArgumentException("Given range [" + From + " - " + value + "] is not valid.");
                }
                _to = value;
            }
        }

        [Browsable(false)]
        public override string Value
        {
            get
            {
                throw new InvalidOperationException("You cannot get a single value for a range.");
            }
            set
            {
                throw new InvalidOperationException("You cannot set a single value for a range.");
            }
        }

        public SymbolRange() { }
        public SymbolRange(string from, string to) : base("<single symbol not valid>")
        {
            if (from == null || from.Length == 0)
                throw new ArgumentException("Range start value cannot be null or empty :\n\r\t" + from + ".");
            if (to == null || to.Length == 0)
                throw new ArgumentException("Range start value cannot be null or empty :\n\r\t" + to + ".");
            if (from.CompareTo(to) > 0)
                throw new ArgumentException("Given range [" + from + " - " + To + "] is not valid.");
            From = from;
            To = to;
        }

        public override TestResult<bool> Matches(string input)
        {
            if (From.Length == 1 && To.Length == 1 && input.Length >= 1)
            {
                char fc = From[0];
                char tc = To[0];
                char ic = input[0];
                if (ic >= fc && ic <= tc)
                {
                    TestResult<bool> result = new TestResult<bool>(true, TestResultType.Complete);
                    if (input.Length > 1)
                        result.Type = TestResultType.Partial;
                    result.Data.Add("Symbol", ic + "");
                    return result;
                }
            }
            else
            {
                int above = input.CompareTo(From);
                int below = input.CompareTo(To);
                if (above >= 0 && below <= 0)
                {
                    TestResult<bool> result = new TestResult<bool>(true, TestResultType.Complete);
                    result.Data.Add("Symbol", input);
                    return result;
                }
            }
            return new TestResult<bool>(false, TestResultType.Failed);
        }

        public override bool Equals(object obj)
        {
            if (_from == null || _to == null)
                return false;
            if (obj.GetType() == typeof(SymbolRange))
                return ((SymbolRange)obj)._from.Equals(_from)
                    && ((SymbolRange)obj)._to.Equals(_to);
            return false;
        }
    }
}

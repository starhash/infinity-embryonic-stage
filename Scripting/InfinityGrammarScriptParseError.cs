using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting
{
    public class InfinityGrammarScriptParseError : Exception
    {
        private string _message;
        private string _line;
        private bool _multiError;
        private List<InfinityGrammarScriptParseError> _errors;

        public InfinityGrammarScriptParseError(string message, string line, Exception innerException = null) : base("", innerException)
        {
            _message = message;
            _line = line;
        }
        public InfinityGrammarScriptParseError(List<InfinityGrammarScriptParseError> errors)
        {
            _errors = errors;
            _message = ((Errors.Count > 1) ? ("Multiple errors. " + _errors.Count + " errors.") : "One error");
            _line = "";
            _multiError = true;
        }
        public InfinityGrammarScriptParseError()
        {
            _message = "";
            _line = "";
        }

        public override string Message
        {
            get
            {
                return _message;
            }
        }

        public string Line
        {
            get
            {
                return _line;
            }
        }

        public bool MultiError
        {
            get
            {
                return _multiError;
            }
        }

        public List<InfinityGrammarScriptParseError> Errors
        {
            get
            {
                return _errors;
            }
        }

        public override string ToString()
        {
            return base.ToString() + "\n" + Message;
        }
    }
}

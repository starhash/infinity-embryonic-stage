using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Grammar;
using Infinity.Engine.Data;
using Infinity.Scripting;

namespace Infinity.Engine.Script
{
    public class ICASMValue
    {
        private object _value;
        public object Value { get { return _value; } set { _value = value; } }

        private ICASMValueType _type;
        public ICASMValueType Type { get { return _type; } set { _type = value; } }

        private ICASMPrimitiveDataType _primitivetype;
        public ICASMPrimitiveDataType PrimitiveType { get { return _primitivetype; } set { _primitivetype = value; } }

        public ICASMValue(ICASMValueType type, object value, ICASMPrimitiveDataType primitivetype = ICASMPrimitiveDataType.Object)
        {
            _type = type;
            _value = value;
            _primitivetype = primitivetype;
        }

        public ICASMValue Check(params ICASMValueType[] type)
        {
            if (type.Contains(this._type)) return this;
            else return null;
        }

        public static Data.Type GetTypeFromPrimitiveType(ICASMPrimitiveDataType primitive)
        {
            switch(primitive)
            {
                case ICASMPrimitiveDataType.Boolean:
                    return TypeEngine.GetType(Address.FromScope(new Scope("/Boolean")));
                case ICASMPrimitiveDataType.Integer:
                    return TypeEngine.GetType(Address.FromScope(new Scope("/Integer")));
                case ICASMPrimitiveDataType.Real:
                    return TypeEngine.GetType(Address.FromScope(new Scope("/Real")));
                case ICASMPrimitiveDataType.String:
                    return TypeEngine.GetType(Address.FromScope(new Scope("/String")));
            }
            return new Data.Type("Object", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace));
        }

        public static ICASMValue ParseValue(string value)
        {
            value = value.Trim();
            bool isType = false;
            if(value.StartsWith("typeof"))
            {
                value = value.Substring(6).Trim();
                if(value.StartsWith("(") && value.EndsWith(")"))
                {
                    value = value.Substring(1, value.Length - 2).Trim();
                    if (value.Equals("stack"))
                        return new ICASMValue(ICASMValueType.Address, RuntimeEngine.GetVariable(ICASMInterpreter.MemoryStack.Peek()).Type.Address.FullPath);
                    else if (value.Equals("stackp"))
                        return new ICASMValue(ICASMValueType.Address, RuntimeEngine.GetVariable(ICASMInterpreter.MemoryStack.Pop()).Type.Address.FullPath);
                    isType = true;
                }
            }
            int intValue; double doubleValue; bool boolValue;
            if (bool.TryParse(value, out boolValue))
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, "/Boolean");
                return new ICASMValue(ICASMValueType.Normal, boolValue, ICASMPrimitiveDataType.Integer);
            }
            else if (int.TryParse(value, out intValue))
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, "/Integer");
                return new ICASMValue(ICASMValueType.Normal, intValue, ICASMPrimitiveDataType.Integer);
            }
            else if (double.TryParse(value, out doubleValue))
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, "/Real");
                return new ICASMValue(ICASMValueType.Normal, doubleValue, ICASMPrimitiveDataType.Real);
            }
            else if (value.StartsWith("'") && value.EndsWith("'") && value.Length == 3)
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, "/Character");
                return new ICASMValue(ICASMValueType.Normal, value[1], ICASMPrimitiveDataType.Character);
            }
            else if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, "/String");
                return new ICASMValue(ICASMValueType.Normal, value.Substring(1, value.Length - 2),
                    ICASMPrimitiveDataType.String);
            }
            LiteralGrammarElement lge = new LiteralGrammarElement("+var", "-var", "+pool", "-pool", "+type", "-type", "+field", "-field", "+fields", "-fields", "+typespace", "-typespace", "-all", "+function", "-function", "call", "assign", "goto", "return", "clear", "if", "elseif", "else", "while", "repeat", "end+");
            string temp = value;
            if(lge.Validate(ref temp, true).Result)
                return new ICASMValue(ICASMValueType.ExecutableResult, value);
            Grammar checker = Infinity.Scripting.InfinityGrammarScript.LoadGrammar("Symbol Underscore = \"_\"\n" + 
                "SymbolSet Digit = new Digit\n" +
                "SymbolSet ForwardSlash = [\"/\"]\n" +
                "SymbolSet IdentifierStartSymbol = new Alphabet, Underscore\n" +
                "SymbolSet IdentifierContinuationSymbol = new Alphabet, new Digit, Underscore\n" +
                "SymbolSet DefaultOperators	= ['+','-','*','/','%','!','@','#','$','^','&','=','|',':','<','>','?']\n"+
                "SymbolSet FunctionIdentifierSymbol = new Alphabet, Underscore, DefaultOperators\n" +
                "GrammarElement Integer = Digit+\n" +
                "GrammarElement Identifier = IdentifierStartSymbol IdentifierContinuationSymbol*\n" +
                "GrammarElement FunctionIdentifier = FunctionIdentifierSymbol FunctionIdentifierSymbol*\n" +
                "GrammarElement AddressNode = ForwardSlash Identifier\n" + 
                "GrammarElement Root = \"/\"\n" +
                "GrammarElement PureAddress = AddressNode AddressNode *\n" + 
                "GrammarElement DotIdentifier = \".\" Identifier\n" +
                "GrammarElement TupleAddress = PureAddress DotIdentifier *\n" +
                "GrammarElement Address = TupleAddress or PureAddress, precedence: Ascending\n" +
                "GrammarElement ParameterAccess = \"[\" Integer \"]\" DotIdentifier*\n" +
                "GrammarElement AddressorRoot = Address or Root, precedence: Ascending\n" +
                "StartSymbol = AddressorRoot");
            temp = value;
            if (checker.Terminals["AddressorRoot"].Validate(ref temp, true).Result)
            {
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, RuntimeEngine.GetVariable(value).Type.Address.FullPath);
                return new ICASMValue(ICASMValueType.Address, value);
            }
            temp = value;
            if (checker.Terminals["ParameterAccess"].Validate(ref temp, true).Result)
            {
                return new ICASMValue(ICASMValueType.FunctionParameterAccess    , value);
            }
            temp = value;
            if (checker.Terminals["Identifier"].Validate(ref temp, true).Result)
            {
                if (value.Equals("stack"))
                    return new ICASMValue(ICASMValueType.Address, ICASMInterpreter.MemoryStack.Peek().FullPath);
                else if (value.Equals("stackp"))
                    return new ICASMValue(ICASMValueType.Address, ICASMInterpreter.MemoryStack.Pop().FullPath);
                if (isType)
                    return new ICASMValue(ICASMValueType.Address, RuntimeEngine.GetVariable("/" + value).Type.Address.FullPath);
                return new ICASMValue(ICASMValueType.Identifier, value);
            }
            temp = value;
            if (checker.Terminals["FunctionIdentifier"].Validate(ref temp, true).Result)
            {
                return new ICASMValue(ICASMValueType.FunctionIdentifier, value);
            }
            return null;
        }
    }

    public enum ICASMValueType { Normal, ExecutableResult, Address, Identifier, FunctionIdentifier, FunctionParameterAccess }
    public enum ICASMPrimitiveDataType
    {
        Object = 0,
        Boolean,
        Integer,
        Real,
        String,
        Character
    }
}

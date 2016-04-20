using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Engine;

namespace Infinity.Engine.Data.DefaultTypes
{
    public class Boolean : Type
    {
        public Boolean()
            : base("Boolean", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace), "System.Boolean")
        {
            Function and = new Function("And", "and", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            and.SingleRepetitiveParameterized = true;
            and.ExecuteFunction += and_ExecuteFunction;
            AddFunction(and);
            
            Function or = new Function("Or", "or", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            or.SingleRepetitiveParameterized = true;
            or.ExecuteFunction += or_ExecuteFunction;
            AddFunction(or);

            Function not = new Function("Not", "not", FunctionType.InfixExpression | FunctionType.Callable, this.Address);
            not.ExecuteFunction += not_ExecuteFunction;
            AddFunction(not);

            Function toString = new Function("ToString", "toString", FunctionType.PostfixExpression, this.Address, Address);
            toString.ExecuteFunction += ToString_ExecuteFunction;
            AddFunction(toString);
            Function fromString = new Function("FromString", "fromString", FunctionType.PostfixExpression, this.Address, "/String");
            fromString.ExecuteFunction += FromString_ExecuteFunction;
            AddFunction(fromString);
        }

        private Variable FromString_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            bool outint;
            bool.TryParse((string)var0.Value, out outint);
            Variable result = new Variable(outint, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable ToString_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable result = new Variable(var0.Value.ToString(), TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable not_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            bool sum = (bool)var.Value;
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable or_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            bool sum = true;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum = sum || (bool)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable and_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            bool sum = true;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum = sum && (bool)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
    }
}

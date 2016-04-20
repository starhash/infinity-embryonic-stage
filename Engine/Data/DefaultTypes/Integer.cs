using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data.DefaultTypes
{
    public class Integer : Type
    {
        public Integer()
            : base("Integer", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace), "System.Int64")
        {
            Function addition = new Function("Addition", "+", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            addition.SingleRepetitiveParameterized = true;
            addition.ExecuteFunction += addition_ExecuteFunction;
            AddFunction(addition);

            Function subtraction = new Function("Subtraction", "-", FunctionType.InfixExpression, this.Address, Address, Address);
            subtraction.ExecuteFunction += subtraction_ExecuteFunction;
            AddFunction(subtraction);

            Function multiplication = new Function("Multiplication", "*", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            multiplication.SingleRepetitiveParameterized = true;
            multiplication.ExecuteFunction += multiplication_ExecuteFunction;
            AddFunction(multiplication);

            Function division = new Function("Division", "/", FunctionType.InfixExpression, this.Address, Address, Address);
            division.ExecuteFunction += division_ExecuteFunction;
            AddFunction(division);

            Function equalSign = new Function("Equality Comparison", "=", FunctionType.InfixExpression, this.Address, Address, Address);
            Function lessThanSign = new Function("Less Than Comparison", "<", FunctionType.InfixExpression, this.Address, Address, Address);
            Function greaterThanSign = new Function("Greater Than Comparison", ">", FunctionType.InfixExpression, this.Address, Address, Address);
            equalSign.ExecuteFunction += EqualSign_ExecuteFunction;
            lessThanSign.ExecuteFunction += LessThanSign_ExecuteFunction;
            greaterThanSign.ExecuteFunction += GreaterThanSign_ExecuteFunction;
            AddFunction(equalSign);
            AddFunction(lessThanSign);
            AddFunction(greaterThanSign);

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
            int outint;
            int.TryParse(var0.Value.ToString(), out outint);
            Variable result = new Variable(outint, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
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
        private Variable GreaterThanSign_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            Variable result = new Variable((int)var0.Value > (int)var1.Value, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            return result;
        }
        private Variable LessThanSign_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            Variable result = new Variable((int)var0.Value > (int)var1.Value, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            return result;
        }
        private Variable EqualSign_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            Variable result = new Variable((int)var0.Value == (int)var1.Value, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Boolean", AddressType.Type)));
            return result;
        }
        Variable division_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            int quotient = 0;
            int remainder = 0;
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            quotient = (int)var0.Value / (int)var1.Value;
            remainder = (int)var0.Value - (quotient * (int)var1.Value);
            Variable result = new Variable(quotient, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            Variable remaindertup = new Variable(remainder, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            result.AddTupleValue("Remainder", remaindertup);
            return result;
        }
        Variable multiplication_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            int sum = 1;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum *= (int)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable subtraction_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            int diff = 0;
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            diff = (int)var0.Value - (int)var1.Value;
            Variable result = new Variable(diff, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable addition_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            int sum = 0;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum += (int)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
    }
}

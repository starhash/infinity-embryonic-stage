using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data.DefaultTypes
{
    public class Real : Type
    {
        public Real()
            : base("Real", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace), "System.Double")
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

            string castFromIntegerName = "$SYSTEM$_Runtime.CastTo_" + Address.FullPath.Replace("/","_");
            Address integerTypeAddress = new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.TypeSpace);
            Function castIntegerToReal = new Function("Cast from Integer to Real", castFromIntegerName, FunctionType.Callable, integerTypeAddress, "/Integer");
            castIntegerToReal.ExecuteFunction += castFromInteger_ExecuteFunction;
            Type integerType = TypeEngine.GetType(integerTypeAddress);
            integerType.AddFunction(castIntegerToReal);

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
            double outint = 0.0;
            if (var0.Value.GetType() == typeof(string))
                double.TryParse((string)var0.Value, out outint);
            else
            {
                string double_str = var0.Value.ToString();
                double.TryParse(double_str, out outint);
            }
            Variable result = new Variable(outint, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Real", AddressType.Type)));
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
        Variable castFromInteger_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable integerOne = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            double temp_double;
            if (integerOne.Value.GetType() == typeof(double))
                temp_double = (double)integerOne.Value;
            else
                temp_double = (int)integerOne.Value;
            int value = (int)temp_double;
            Double real = value * 1.0;
            Type realType = TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Real", AddressType.Type));
            Variable realOne = new Variable(real, realType);
            RuntimeEngine.PutVariable(_destination_address, realOne);
            return realOne;
        }
        Variable division_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Double quotient = 0;
            Double remainder = 0;
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            quotient = (Double)var0.Value / (Double)var1.Value;
            remainder = (Double)var0.Value - (quotient * (Double)var1.Value);
            Variable result = new Variable(quotient, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            Variable remaindertup = new Variable(remainder, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            result.AddTupleValue("Remainder", remaindertup);
            return result;
        }
        Variable multiplication_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Double sum = 1;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum *= (Double)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable subtraction_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Double diff = 0;
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable var1 = RuntimeEngine.GetVariable(_parameter_addresses[1]);
            diff = (Double)var0.Value - (Double)var1.Value;
            Variable result = new Variable(diff, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        Variable addition_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Double sum = 0;
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                double tempval;
                if (var.Value.GetType() == typeof(int))
                    tempval = (int)var.Value;
                else
                    tempval = (Double)var.Value;
                sum += tempval;
                if(var.Address.Name.Contains("$SYSTEM$_temp"))
                    RuntimeEngine.GetPool(addr.Parent).Pull(addr.Name);
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
    }
}

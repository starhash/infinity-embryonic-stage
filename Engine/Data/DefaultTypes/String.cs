using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data.DefaultTypes
{
    public class String : Type
    {
        public String()
            : base("String", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace), "System.String")
        {
            Function length = new Function("Length", "#", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            length.SingleRepetitiveParameterized = false;
            length.ExecuteFunction += Length_ExecuteFunction;
            AddFunction(length);

            Function concatenation = new Function("Concatenation", "+", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            concatenation.SingleRepetitiveParameterized = true;
            concatenation.ExecuteFunction += Concatenation_ExecuteFunction;
            AddFunction(concatenation);

            Function removal = new Function("Removal", "-", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            removal.SingleRepetitiveParameterized = true;
            removal.ExecuteFunction += Removal_ExecuteFunction;
            AddFunction(removal);

            Function multipleconcat = new Function("MultipleConcatenation", "*", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            multipleconcat.SingleRepetitiveParameterized = true;
            multipleconcat.ExecuteFunction += Multipleconcat_ExecuteFunction;
            AddFunction(multipleconcat);

            Function split = new Function("Split", "/", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address);
            split.SingleRepetitiveParameterized = true;
            split.ExecuteFunction += Split_ExecuteFunction;
            AddFunction(split);

            Function indexof = new Function("IndexOf", "indexof", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, Address);
            indexof.SingleRepetitiveParameterized = false;
            indexof.ExecuteFunction += Indexof_ExecuteFunction;
            AddFunction(indexof);
            Function indexofafter = new Function("IndexOf", "indexof", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, Address, "/Integer");
            indexofafter.SingleRepetitiveParameterized = false;
            indexofafter.ExecuteFunction += Indexof_ExecuteFunction;
            AddFunction(indexofafter);

            Function lastindexof = new Function("LastIndexOf", "lastindexof", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, Address);
            lastindexof.SingleRepetitiveParameterized = false;
            lastindexof.ExecuteFunction += Lastindexof_ExecuteFunction;
            AddFunction(lastindexof);
            Function lastindexofafter = new Function("LastIndexOf", "lastindexof", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, Address, "/Integer");
            lastindexofafter.SingleRepetitiveParameterized = false;
            lastindexofafter.ExecuteFunction += Lastindexof_ExecuteFunction;
            AddFunction(lastindexofafter);

            Function substring = new Function("Substring", ":", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, "/Integer");
            substring.SingleRepetitiveParameterized = false;
            substring.ExecuteFunction += Substring_ExecuteFunction; ;
            AddFunction(substring);
            Function substringfromto = new Function("Substring", ":", FunctionType.InfixExpression | FunctionType.Callable, this.Address, Address, "/Integer", "/Integer");
            substringfromto.SingleRepetitiveParameterized = false;
            substringfromto.ExecuteFunction += Substring_ExecuteFunction;
            AddFunction(substringfromto);

            Function toString = new Function("ToString", "toString", FunctionType.PostfixExpression, this.Address, Address);
            toString.ExecuteFunction += ToString_ExecuteFunction;
            AddFunction(toString);
        }

        private Variable ToString_ExecuteFunction1(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            throw new NotImplementedException();
        }

        private Variable ToString_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            Variable var0 = RuntimeEngine.GetVariable(_parameter_addresses[0]);
            Variable result = new Variable(var0.Value.ToString(), TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }

        private Variable Length_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string str = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            Variable result = new Variable(str.Length, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable Substring_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string str = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            int from = (int)RuntimeEngine.GetVariable(_parameter_addresses[1]).Value;
            int to = str.Length;
            if (_parameter_addresses.Length > 2)
            {
                to = (int)RuntimeEngine.GetVariable(_parameter_addresses[2]).Value;
            }
            str = str.Substring(from, to - from);
            Variable result = new Variable(str, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable Lastindexof_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string str = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            string of = (string)RuntimeEngine.GetVariable(_parameter_addresses[1]).Value;
            int idx = str.Length - 1;
            if (_parameter_addresses.Length > 2)
            {
                idx = (int)RuntimeEngine.GetVariable(_parameter_addresses[2]).Value;
            }
            idx = str.LastIndexOf(of, idx);
            Variable result = new Variable(idx, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable Indexof_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string str = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            string of = (string)RuntimeEngine.GetVariable(_parameter_addresses[1]).Value;
            int idx = 0;
            if (_parameter_addresses.Length > 2)
            {
                idx = (int)RuntimeEngine.GetVariable(_parameter_addresses[2]).Value;
            }
            idx = str.IndexOf(of, idx);
            Variable result = new Variable(idx, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "Integer", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable Split_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string concatto = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            string[] splistrs = new string[_parameter_addresses.Length - 1];
            Variable result = new Variable();
            RuntimeEngine.PutVariable(_destination_address, result);
            for (int i = 1; i < _parameter_addresses.Length; i++)
            {
                string str = (string)RuntimeEngine.GetVariable(_parameter_addresses[i]).Value;
                splistrs[i - 1] = str;
            }
            string[] splits = concatto.Split(splistrs, StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i<splits.Length; i++)
            {
                Variable concat = new Variable(splits[i], TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
                result.AddTupleValue(i + "", concat);
            }
            return result;
        }
        private Variable Multipleconcat_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string concatto = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            Variable result = new Variable();
            RuntimeEngine.PutVariable(_destination_address, result);
            for (int i = 1; i < _parameter_addresses.Length; i++)
            {
                Variable concat = new Variable(concatto + (string)RuntimeEngine.GetVariable(_parameter_addresses[i]).Value,
                    TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
                result.AddTupleValue((i - 1) + "", concat);
            }
            return result;
        }
        private Variable Removal_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string removed = (string)RuntimeEngine.GetVariable(_parameter_addresses[0]).Value;
            for(int i = 1; i<_parameter_addresses.Length; i++)
            {
                string toremove = (string)RuntimeEngine.GetVariable(_parameter_addresses[i]).Value;
                removed = removed.Replace(toremove, "");
            }
            Variable result = new Variable(removed, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
        private Variable Concatenation_ExecuteFunction(Function _executing_function, Address _destination_address, params Address[] _parameter_addresses)
        {
            string sum = "";
            foreach (Address addr in _parameter_addresses)
            {
                Variable var = RuntimeEngine.GetVariable(addr);
                sum += (string)var.Value;
            }
            Variable result = new Variable(sum, TypeEngine.GetType(new Address("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, "String", AddressType.Type)));
            RuntimeEngine.PutVariable(_destination_address, result);
            return result;
        }
    }
}

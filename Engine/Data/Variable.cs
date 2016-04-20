using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data
{
    public class Variable : Addressable
    {
        private object _value;
        private Type _type;
        private Dictionary<string, Address> _tuple_addresses;
        private int _temps_declared = 0;

        public Dictionary<string, Address> TupleAddresses
        {
            get
            {
                return _tuple_addresses;
            }
        }
        public object Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        public Type Type { get { return _type; } set { _type = value; } }
        public void AddTupleValue(string name, Variable variable)
        {
            Address addr_tuple_value = new Address(Address.Path, Address.Name + "." + name, AddressType.Variable);
            RuntimeEngine.PutVariable(addr_tuple_value, variable);
            _tuple_addresses.Add(name, addr_tuple_value);
        }
        public Variable this[string tuple_field_name]
        {
            get
            {
                Address addr = new Address(Address.Path, Address.Name + "." + tuple_field_name, AddressType.Variable);
                return RuntimeEngine.GetVariable(addr);
            }
        }

        public Variable()
        {
            _tuple_addresses = new Dictionary<string, Address>();
        }
        public Variable(object value, Type type, params Address[] _tuple_value_addresses)
        {
            _tuple_addresses = new Dictionary<string, Address>();
            _value = value;
            _type = type;
            foreach (Address addr in _tuple_value_addresses)
            {
                _tuple_addresses.Add(addr.Name, addr);
            }
        }

        public Variable ExecuteFunction(Address destination, string accesskey, params Address[] _addresses)
        {
            List<Address> addrs = _addresses.ToList();
            addrs.Insert(0, this.Address);
            _addresses = addrs.ToArray();
            addrs = null;
            return _type.ExecuteFunction(destination, accesskey, _addresses);
        }
        public Variable ExecuteFunction(string accesskey, params Address[] _addresses)
        {
            List<Address> addrs = _addresses.ToList();
            addrs.Insert(0, this.Address);
            _addresses = addrs.ToArray();
            addrs = null;
            Address destination = new Address(Address.Path, Address.Name + ".$SYSTEM$__temp" + (_temps_declared++), AddressType.Variable);
            return _type.ExecuteFunction(destination, accesskey, _addresses);
        }

        public override string ToString()
        {
            return _address + " = " + _value + ", Contains " + _tuple_addresses.Count + " tuples.";
        }
    }
}

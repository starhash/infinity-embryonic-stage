using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data
{
    public class TypeSpace
    {
        private Dictionary<string, Type> _typespace;
        private Dictionary<string, TypeSpace> _sub_typespace;
        private string _typespace_name;
        private Address _typespace_address;

        public TypeSpace(string name, Address parent)
        {
            _typespace_name = name;
            _typespace_address = new Address(parent.Path + "/" + parent.Name, name, AddressType.TypeSpace);
            _typespace = new Dictionary<string, Type>();
            _sub_typespace = new Dictionary<string, TypeSpace>();
        }

        public Type Get(string name)
        {
            return _typespace[name];
        }
        public Type Create(string name, string _igml_code)
        {
            Type new_type = new Type(name, _typespace_address);

            _typespace.Add(name, new_type);
            return new_type;
        }
        public void Put(string name, Type type)
        {
            _typespace.Add(name, type);
        }
        public Type Pull(string name)
        {
            Type pulled = _typespace[name];
            _typespace.Remove(name);
            return pulled;
        }
        public TypeSpace PullTypeSpace(string name)
        {
            TypeSpace space = _sub_typespace[name];
            _sub_typespace.Remove(name);
            return space;
        }
        public bool HasType(string name)
        {
            return _typespace.ContainsKey(name);
        }

        public TypeSpace CreateTypeSpace(string name)
        {
            TypeSpace new_ts = new TypeSpace(name, _typespace_address);
            _sub_typespace.Add(name, new_ts);
            return new_ts;
        }
        public TypeSpace GetTypeSpace(string name)
        {
            return _sub_typespace[name];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data
{
    public class Type : Addressable
    {
        protected string _type_name;
        protected string _equivalentSystemType;
        protected Dictionary<string, FunctionOverloads> _functions;
        protected Queue<string> _memberCreationQueue;

        public string Name { get { return _type_name; } }
        public Queue<string> MemberCreationQueue { get { return _memberCreationQueue; } }
        public Dictionary<string, FunctionOverloads> Functions { get { return _functions; } }
        public int FunctionCount { get { return _functions.Count; } }
        
        public Type(string name, Address parent, string equivalentSystemType = "System.Object")
        {
            _type_name = name;
            _address = new Address(parent.Path + "/" + parent.Name, name, AddressType.Type);
            _functions = new Dictionary<string, FunctionOverloads>();
            _memberCreationQueue = new Queue<string>();
        }

        public bool Equals(Type type)
        {
            return Address.Name.Equals(type.Address.Name) && Address.Path.Equals(type.Address.Path)
                && Address.Type == type.Address.Type;
        }

        public bool HasCastTo(Address address)
        {
            return HasCastTo(TypeEngine.GetType(address));
        }

        public bool HasCastTo(Type type)
        {
            string temp = "$SYSTEM$_Runtime.CastTo_" + type.Address.FullPath.Replace("/", "_");
            if (_functions.ContainsKey(temp) || type._functions.ContainsKey(temp))
            {
                return true;
            }
            return false;
        }

        public void AddFunction(Function function)
        {
            if (_functions.ContainsKey(function.AccessKeyword))
            {
                _functions[function.AccessKeyword].AddFunction(function);
            }
            else
            {
                FunctionOverloads fov = new FunctionOverloads(function.AccessKeyword);
                fov.AddFunction(function);
                _functions.Add(fov.AccessKey, fov);
            }
        }
        public Variable ExecuteFunction(Address _destination_address, string accesskey, params Address[] _addresses)
        {
            return _functions[accesskey].GetMatch(_addresses).Execute(_destination_address, _addresses);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting;

namespace Infinity.Engine.Data
{
    public class Address
    {
        private string _path;
        private string _variable_name;
        private AddressType _type;
        public string Path { get { return _path; } }
        public string Name { get { return _variable_name; } }
        public AddressType Type { get { return _type; } }

        public Address(string path, string name, AddressType type)
        {
            if (path == null || path.Trim().Trim('/').Length == 0)
                _path = "";
            else
                _path = path;
            _variable_name = name;
            _type = type;
        }
        public Address(AddressType type)
        {
            _path = "";
            _variable_name = "";
            _type = type;
        }

        public override string ToString()
        {
            return FullPath;
        }

        public static Address FromScope(Scope scope)
        {
            Address address = new Address((scope.Parent == null)?null:scope.Parent.Path, scope.TargetName, AddressType.VariablePool);
            return address;
        }

        public Scope ToScope()
        {
            return new Scope(FullPath);
        }

        public Address GetChild(string name)
        {
            if ((_path == null || _path == "") && (_variable_name == null || _variable_name == ""))
                return new Address("", name, _type);
            return new Address(_path + "/" + _variable_name, name, _type);
        }

        public string FullPath
        {
            get
            {
                string fp = (_path == "" || _path == null) ? "" : (_path);
                fp = fp + "/" + _variable_name;
                return fp;
            }
        }

        public Address Parent
        {
            get
            {
                Scope here = ToScope();
                if (here.Parent == null)
                    return new Address("", "", AddressType.Variable);
                here = here.Parent;
                return Address.FromScope(here);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Address)) return false;
            string s = FullPath.Replace("/$SYSTEM$_Runtime.TypeSpace@TypeEngine", "");
            string s2 = ((Address)obj).FullPath.Replace("/$SYSTEM$_Runtime.TypeSpace@TypeEngine", "");
            return s.Equals(s2);
        }

        public static implicit operator Address(string address)
        {
            return Address.FromScope(new Scope(address));
        }

        public static implicit operator string(Address address)
        {
            return address.FullPath;
        }
    }
}

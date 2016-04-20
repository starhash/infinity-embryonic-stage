using System;
using Infinity.Engine.Data;

namespace Infinity.Engine
{
    public class TypeEngine
    {
        static TypeEngine()
        {
            TypeEngine.AddType(new Address(AddressType.Type), new Infinity.Engine.Data.DefaultTypes.Object());
            TypeEngine.AddType(new Address(AddressType.Type), new Infinity.Engine.Data.DefaultTypes.Boolean());
            TypeEngine.AddType(new Address(AddressType.Type), new Infinity.Engine.Data.DefaultTypes.Integer());
            TypeEngine.AddType(new Address(AddressType.Type), new Infinity.Engine.Data.DefaultTypes.Real());
            TypeEngine.AddType(new Address(AddressType.Type), new Infinity.Engine.Data.DefaultTypes.String());
        }

        private static TypeSpace _typespace;
        public static TypeSpace RuntimeTypes
        {
            get
            {
                if (_typespace == null)
                    _typespace = new TypeSpace("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, new Address("", "", AddressType.TypeSpace));
                return _typespace;
            }
        }

        public static Infinity.Engine.Data.Type GetType(Address _type_address)
        {
            TypeSpace typespace = GetTypeSpace(_type_address.Parent);
            return typespace.Get(_type_address.Name);
        }
        public static void CreateType(Address _type_address, string igml_code)
        {
            TypeSpace typespace = GetTypeSpace(_type_address);
            typespace.Create(_type_address.Name, igml_code);
        }
        public static void AddType(Address _type_address, Infinity.Engine.Data.Type type)
        {
            TypeSpace typespace = GetTypeSpace(_type_address.Parent);
            typespace.Put(type.Address.Name, type);
        }
        public static void CreateTypeSpace(Address _typespace_address)
        {
            TypeSpace typespace = GetTypeSpace(_typespace_address);
            typespace.CreateTypeSpace(_typespace_address.Name);
        }
        public static TypeSpace GetTypeSpace(Address _typespace_address)
        {
            string[] _nodes = _typespace_address.FullPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            TypeSpace typespace = TypeEngine.RuntimeTypes;
            int i = 0;
            if (_nodes.Length >= 1)
            {
                if (_nodes[0].Equals("$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name))
                {
                    i = 1;
                }
            }
            for (; i < _nodes.Length; i++)
            {
                typespace = typespace.GetTypeSpace(_nodes[i]);
            }
            return typespace;
        }
    }
}

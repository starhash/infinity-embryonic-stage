using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data.DefaultTypes
{
    public class Object : Type
    {
        public Object()
            : base("Object", new Address("", "$SYSTEM$_Runtime.TypeSpace@" + typeof(TypeEngine).Name, AddressType.TypeSpace), "System.Object")
        {

        }
    }
}

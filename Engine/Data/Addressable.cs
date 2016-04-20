using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Data
{
    public abstract class Addressable
    {
        protected Address _address;
        public Address Address { get { return _address; } set { _address = value; } }
    }
}

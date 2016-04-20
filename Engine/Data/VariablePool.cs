using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting;

namespace Infinity.Engine.Data
{
    public class VariablePool
    {
        private Dictionary<string, Variable> _variable_pool;
        private Dictionary<string, VariablePool> _variable_pool_cluster;
        private string _variable_pool_name;
        private Address _pool_address;

        public Address Address
        {
            get { return _pool_address; }
        }
        public int VariableCount { get { return _variable_pool.Count; } }
        public int VariablePoolCount { get { return _variable_pool_cluster.Count; } }
        public Dictionary<string, Variable> Variables { get { return _variable_pool; } }
        public Dictionary<string, VariablePool> VariablePools {  get { return _variable_pool_cluster; } }

        public VariablePool(string name, Address parent)
        {
            _variable_pool_name = name;
            _pool_address = new Address(parent.Path + "/" + parent.Name, name, AddressType.VariablePool);
            _variable_pool = new Dictionary<string, Variable>();
            _variable_pool_cluster = new Dictionary<string, VariablePool>();
        }

        public Variable Get(string name) { return _variable_pool[name]; }
        public void Set(string name, Variable variable) { _variable_pool[name] = variable; }
        public void Put(string name, Variable variable)
        {
            string prev_name = "";
            if(variable.Address != null)
                prev_name = variable.Address.Name;
            if (name.StartsWith("$SYSTEM$_temp") && _variable_pool.ContainsKey(name))
                _variable_pool.Remove(name);
            _variable_pool.Add(name, variable);
            _variable_pool[name].Address = new Address(_pool_address.Path + "/" + _pool_address.Name, name, AddressType.Variable); 
            Variable temp = _variable_pool[name];
            if (prev_name.Length != 0)
            {
                for (int i = 0; i < temp.TupleAddresses.Count; i++)
                {
                    string key = temp.TupleAddresses.Keys.ElementAt(i);
                    Address address = temp.TupleAddresses[key];
                    Variable tupvar = this.Pull(address.Name);
                    string newname = address.Name.Replace(prev_name, name);
                    address = new Address(address.Path, newname, address.Type);
                    temp.TupleAddresses[key] = address;
                    Put(newname, tupvar);
                }
                if (!prev_name.Equals(name) && _variable_pool.ContainsKey(prev_name))
                {
                    _variable_pool.Remove(prev_name);
                }
            }
        }
        public Variable Pull(string name, bool suppress = false)
        {
            if (suppress && !_variable_pool.ContainsKey(name))
            {
                return null;
            }
            Variable v = _variable_pool[name];
            _variable_pool.Remove(name);
            foreach(KeyValuePair<string, Address> value in v.TupleAddresses)
            {
                _variable_pool.Remove(value.Value.Name);
            }
            return v;
        }
        public VariablePool PullPool(string name) { VariablePool v = _variable_pool_cluster[name]; _variable_pool_cluster.Remove(name); return v; }
        public bool HasVariable(string name) { return _variable_pool.ContainsKey(name); }

        public VariablePool CreatePool(string name) 
        { 
            _variable_pool_cluster.Add(name, new VariablePool(name, _pool_address)); 
            return _variable_pool_cluster[name]; 
        }
        public VariablePool GetPool(string name) { return _variable_pool_cluster[name]; }
        public bool HasPool(string name) { return _variable_pool_cluster.ContainsKey(name); }
    }
}

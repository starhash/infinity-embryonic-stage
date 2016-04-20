using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Engine.Data;
using Infinity.Scripting;

namespace Infinity.Engine
{
    public class RuntimeEngine
    {
        private static VariablePool _variable_pool;
        public static VariablePool VariablePool
        {
            get
            {
                if (_variable_pool == null)
                    _variable_pool = new VariablePool("$SYSTEM$_Runtime.Pool@" + typeof(RuntimeEngine).Name, new Address("", "", AddressType.VariablePool));
                return _variable_pool;
            }
        }

        public static Variable GetVariable(Address _variable_address)
        {
            VariablePool varpool = GetPool(_variable_address.Parent);
            return varpool.Get(_variable_address.Name);
        }
        public static void PutVariable(Address _variable_address, Variable variable, bool auto_purge = true)
        {
            VariablePool varpool = GetPool(_variable_address.Parent);
            if (auto_purge && variable.Address != null)
            {
                if(variable.Address.Name.Contains("$SYSTEM$__temp"))
                    if(varpool.HasVariable(variable.Address.Name))
                        varpool.Pull(variable.Address.Name);
            }
            varpool.Put(_variable_address.Name, variable);
            variable.Address = _variable_address;
        }
        public static void SetVariable(Address _variable_address, Variable variable)
        {
            VariablePool varpool = GetPool(_variable_address.Parent);
            Variable temp = varpool.Pull(_variable_address.Name);
            varpool.Put(_variable_address.Name, variable);
        }
        public static VariablePool CreatePool(Address _pool_address)
        {
            Scope scope = _pool_address.ToScope();
            List<string> levels = scope.ToList();
            levels.Reverse();
            VariablePool runtimePool = RuntimeEngine.VariablePool;
            foreach (string level in levels)
            {
                if (!runtimePool.HasPool(level))
                    runtimePool.CreatePool(level);
                runtimePool = runtimePool.GetPool(level);
            }
            return runtimePool;
        }
        public static VariablePool GetPool(Address addr)
        {
            string[] _nodes = addr.FullPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            VariablePool varpool = RuntimeEngine.VariablePool;
            int i = 0;
            if (_nodes.Length >= 1)
            {
                if (_nodes[0].Equals("$SYSTEM$_Runtime.Pool@" + typeof(RuntimeEngine).Name))
                {
                    i = 1;
                }
            }
            for (; i < _nodes.Length; i++)
            {
                varpool = varpool.GetPool(_nodes[i]);
            }
            return varpool;
        }
    }
}

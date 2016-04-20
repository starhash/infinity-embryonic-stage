using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting.Grammar.Parsing
{
    public class TreeNode<T>
    {
        protected T _value;
        protected List<TreeNode<T>> _children;

        public T Value { get { return _value; } set { _value = value; } }
        public List<TreeNode<T>> Children { get { return _children; } }

        public TreeNode()
        {
            _children = new List<TreeNode<T>>();
        }
    }
}

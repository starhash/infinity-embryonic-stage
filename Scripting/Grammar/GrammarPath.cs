using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Scripting.Grammar
{
    public class GrammarPath
    {
        private Queue<string> _nodes;
        private string _path;
        public string Path
        {
            get
            {
                _path = "";
                foreach (string s in _nodes)
                {
                    _path = "/" + s + _path;
                }
                return _path;
            }
        }

        public GrammarPath()
        {
            _nodes = new Queue<string>();
        }

        public void Put(string node)
        {
            _nodes.Enqueue(node);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace Infinity.Scripting
{
    public class Scope
    {
        private Stack<string> _scopeStack;

        public Scope(string path = "")
        {
            _scopeStack = new Stack<string>();
            string[] _scopes = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string scope in _scopes)
            {
                _scopeStack.Push(scope);
            }
        }

        public string Path
        {
            get
            {
                string path = "";
                foreach (string s in _scopeStack)
                {
                    path = "/" + s + path;
                }
                return path;
            }
        }

        public Scope Parent
        {
            get
            {
                Scope parent = new Scope(this.Path);
                if(parent._scopeStack.Count != 0)
                    parent._scopeStack.Pop();
                if (parent._scopeStack.Count == 0) return null;
                return parent;
            }
        }

        public string TargetName
        {
            get
            {
                if (_scopeStack.Count == 0) return null;
                return _scopeStack.Peek();
            }
        }

        public Scope Child(string name)
        {
            Scope child = new Scope(this.Path);
            child._scopeStack.Push(name);
            return child;
        }

        public List<string> ToList()
        {
            return _scopeStack.ToList();
        }

        public static implicit operator Scope(string address)
        {
            return new Scope(address);
        }
    }
}

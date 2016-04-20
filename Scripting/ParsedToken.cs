using System;
using System.Xml;

namespace Infinity.Scripting
{
    public class ParsedToken
    {
        private Enum _type;
        private XmlNode _token_node;
        public Enum Type { get { return _type; } }

        public delegate void InterpretAction(ParsedToken _token);
        public event InterpretAction OnInterpret;

        public ParsedToken(Enum _type)
        {
            this._type = _type;
        }
        public ParsedToken(Enum _type, XmlNode _token_node)
        {
            this._type = _type;
            this._token_node = _token_node;
        }

        public void Interpret()
        {
            OnInterpret(this);
        }
    }
}

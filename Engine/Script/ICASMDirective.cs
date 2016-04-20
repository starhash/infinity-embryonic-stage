using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infinity.Engine.Script
{
    public class ICASMDirective
    {
        private ICASMDirectiveType _type;
        public ICASMDirectiveType Type { get { return _type; } }

        private ICASMTagType _tag;
        public ICASMTagType Tag { get { return _tag; } }

        private ICASMDirectiveParameters _parameters;
        public ICASMDirectiveParameters Parameters { get { return _parameters; } }

        public ICASMDirective(ICASMDirectiveType type, ICASMTagType tag, ICASMDirectiveParameters parameters)
        {
            _type = type;
            _tag = tag;
            _parameters = parameters;
        }

        public static ICASMDirectiveType GetTypeFromString(string type)
        {
            type = type.Replace("import", "Import");
            type = type.Replace("reflect", "Reflect");
            type = type.Replace("ewfc", "Ewfc");
            type = type.Replace("var", "Variable");
            type = type.Replace("pool", "VariablePool");
            type = type.Replace("type", "Type");
            type = type.Replace("field", "Field");
            type = type.Replace("fields", "Fields");
            type = type.Replace("typespace", "Typespace");
            type = type.Replace("function", "Function");
            type = type.Replace("call", "Call");
            type = type.Replace("-all", "-All");
            type = type.Replace("assign", "Assign");
            type = type.Replace("jump", "Jump");
            type = type.Replace("return", "Return");
            type = type.Replace("clear", "Clear");
            type = type.Replace("if", "If");
            type = type.Replace("elseif", "ElseIf");
            type = type.Replace("else", "Else");
            type = type.Replace("while", "While");
            type = type.Replace("repeat", "Repeat");
            type = type.Replace("end+", "End");
            if (type.StartsWith("+") || type.StartsWith("-"))
            {
                char action = type[0];
                type = type.Substring(1).Trim() + ((action == '+')?"Add":"Remove");
            }

            type = type + "Directive";
            return (ICASMDirectiveType)Enum.Parse(typeof(ICASMDirectiveType), type);
        }
        public static ICASMTagType GetTagTypeFromString(string tag)
        {
            tag = tag.Substring(1);
            tag = tag.Substring(0, 1).ToUpper() + tag.Substring(1);
            return (ICASMTagType)Enum.Parse(typeof(ICASMTagType), tag);
        }
        public void SetTag(ICASMTagType tagType)
        {
            _tag = tagType;
        }
    }
}

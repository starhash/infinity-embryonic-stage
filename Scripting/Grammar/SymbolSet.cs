using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Design;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SymbolSet : List<Symbol>
    {
        private SymbolSetPredefinedType _type = SymbolSetPredefinedType.None;
        private List<Symbol> _symbolstemp;
        private List<SymbolRange> _symbolrangestemp;

        [Browsable(true)]
        [Description("Collection of symbols")]
        [Category("Set")]
        [DisplayName("Symbols")]
        [Editor(typeof(SymbolCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BindingList<Symbol> Symbols
        {
            get
            {
                _symbolstemp = new List<Symbol>();
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].GetType() == typeof(Symbol))
                    {
                        _symbolstemp.Add(this[i]);
                    }
                }
                return new BindingList<Symbol>(_symbolstemp);
            }
        }

        [Browsable(true)]
        [Description("Collection of symbol ranges")]
        [Category("Set")]
        [DisplayName("Symbol Ranges")]
        [Editor(typeof(SymbolRangeCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public BindingList<SymbolRange> SymbolRanges
        {
            get
            {
                _symbolrangestemp = new List<SymbolRange>();
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].GetType() == typeof(SymbolRange))
                        _symbolrangestemp.Add((SymbolRange)this[i]);
                }
                return new BindingList<SymbolRange>(_symbolrangestemp);
            }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The default type of the symbol set.")]
        [Category("Set")]
        [DisplayName("Type")]
        public SymbolSetPredefinedType Type
        {
            get { return _type; }
            set { _type = value; SymbolSet temp = SymbolSet.FromType(value); this.Clear(); this.AddRange(temp); }
        }

        public TestResult<bool> IsSatisfiedBy(string str)
        {
            foreach (Symbol sym in this)
            {
                TestResult<bool> result = sym.Matches(str);
                if (result.Result)
                {
                    result.Data.Add("SymbolObject", sym);
                    return result;
                }
            }
            return new TestResult<bool>(false, TestResultType.Failed);
        }

        public static SymbolSet FromType(SymbolSetPredefinedType type)
        {
            SymbolSet s = new SymbolSet();
            s._type = type;
            if (type == SymbolSetPredefinedType.Alphabet)
            {
                s.Add(new SymbolRange("A", "Z"));
                s.Add(new SymbolRange("a", "z"));
            }
            else if (type == SymbolSetPredefinedType.AlphabetUppercase)
            {
                s.Add(new SymbolRange("A", "Z"));
            }
            else if (type == SymbolSetPredefinedType.AlphabetLowercase)
            {
                s.Add(new SymbolRange("a", "z"));
            }
            else if (type == SymbolSetPredefinedType.Digit)
            {
                s.Add(new SymbolRange("0", "9"));
            }
            else if (type == SymbolSetPredefinedType.AlphaNumeric)
            {
                s.Add(new SymbolRange("A", "Z"));
                s.Add(new SymbolRange("a", "z"));
                s.Add(new SymbolRange("0", "9"));
            }
            else if (type == SymbolSetPredefinedType.Symbol)
            {
                s.Add(new SymbolRange((char)32 + "", (char)47 + ""));
                s.Add(new SymbolRange((char)58 + "", (char)64 + ""));
                s.Add(new SymbolRange((char)91 + "", (char)96 + ""));
                s.Add(new SymbolRange((char)123 + "", (char)191 + ""));
            }
            else if (type == SymbolSetPredefinedType.EscapeSequences)
            {
                s.Add(new Symbol((char)9));
                s.Add(new Symbol((char)10));
            }
            return s;
        }

        public int RemoveSymbol(Symbol s)
        {
            foreach (Symbol sym in this)
            {
                if (sym.Matches(s.Value).Result)
                {
                    int idx = IndexOf(sym);
                    if (sym.GetType() == typeof(SymbolRange))
                    {
                        if(s.Value.Length == 1)
                        {
                            char c = s.Value[0];
                            SymbolRange ss = (SymbolRange)sym;
                            SymbolRange s1 = new SymbolRange(ss.From, (char)(c - 1) + "");
                            SymbolRange s2 = new SymbolRange((char)(c + 1) + "", ss.To);
                            Add(s1);
                            Add(s2);
                            Remove(sym);
                        }
                    }
                    else if(sym.GetType() == typeof(Symbol))
                        Remove(sym);
                    return idx;
                }
            }
            return -1;
        }
    }

    public class SymbolCollectionEditor : CollectionEditor
    {
        public SymbolCollectionEditor() : base(typeof(List<Symbol>)) { }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object result = base.EditValue(context, provider, value);
            Type contextType = context.Instance.GetType();
            Type valueType = value.GetType();
            SymbolSet symset = (SymbolSet)context.Instance;
            foreach (Symbol s in ((BindingList<Symbol>)value))
            {
                if (!symset.Contains(s))
                    symset.Add(s);
            }
            return result;
        }
    }

    public class SymbolRangeCollectionEditor : CollectionEditor
    {
        public SymbolRangeCollectionEditor() : base(typeof(List<SymbolRange>)) { }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object result = base.EditValue(context, provider, value);
            Type contextType = context.Instance.GetType();
            Type valueType = value.GetType();
            SymbolSet symset = (SymbolSet)context.Instance;
            symset.AddRange(((BindingList<SymbolRange>)value).ToList());
            return result;
        }
    }
}

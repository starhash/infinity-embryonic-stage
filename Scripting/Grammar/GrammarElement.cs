using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class GrammarElement
    {
        protected SymbolSetCollection Sets = new SymbolSetCollection();
        protected bool _is_interim_token;
        protected string _elementname;

        [Browsable(true)]
        [ReadOnly(true)]
        [Description("The name through which this element is identified")]
        [Category("General")]
        [DisplayName("Name")]
        public string Name
        {
            get { return _elementname; }
            set { _elementname = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("Not implemented")]
        [Category("General")]
        [DisplayName("IsInterim")]
        public bool IsInterim
        {
            get { return _is_interim_token; }
            set { _is_interim_token = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The collection of symbol sets, out of which one has to be satisfied")]
        [Category("General")]
        [DisplayName("Sets")]
        [Editor(typeof(CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public virtual SymbolSetCollection SetItems
        {
            get { return Sets; }
            set { Sets = value; }
        }

        public abstract TestResult<bool> Validate(ref string input, bool consume);

        public virtual void AddElement(SymbolSet set)
        {
            Sets.Add(set);
        }
    }

    public class GrammarElementCollectionEditor : CollectionEditor
    {
        public GrammarElementCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] {
                typeof(LiteralGrammarElement), 
                typeof(VariableLengthGrammarElement), 
                typeof(CompoundGrammarElement), 
                typeof(MultiParseGrammarElement), 
                typeof(RepetitiveGrammarElement)
            };
        }
    }
}

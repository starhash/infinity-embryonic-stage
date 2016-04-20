using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Infinity.Scripting.Utils;

namespace Infinity.Scripting.Grammar
{
    public class Grammar
    {
        protected Dictionary<string, Symbol> _symbols;
        protected Dictionary<string, SymbolSet> _symbolsets;
        protected Dictionary<string, GrammarElement> _terminals;
        protected string _startterminal;
        protected string _name;


        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The symbols in the grammar")]
        [Category("General")]
        [DisplayName("Symbols")]
        public Dictionary<string, Symbol> Symbols { get { return _symbols; } }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The symbol sets in the grammar")]
        [Category("General")]
        [DisplayName("Symbol Sets")]
        public Dictionary<string, SymbolSet> SymbolSets { get { return _symbolsets; } }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The terminals in the grammar")]
        [Category("General")]
        [DisplayName("Terminals")]
        public Dictionary<string, GrammarElement> Terminals { get { return _terminals; } }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("Start Symbol of grammar")]
        [Category("General")]
        [DisplayName("Start Symbol")]
        public string Start
        {
            get { return _startterminal; }
            set
            {
                if (Terminals.Keys.Contains(value))
                    _startterminal = value;
                else
                    throw new KeyNotFoundException("There is no such terminal in the current grammar as " + value);
            }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Description("The name of the grammar")]
        [Category("General")]
        [DisplayName("Name")]
        public string Name { get { return _name; } set { _name = value; } }

        public Grammar()
        {
            _symbols = new Dictionary<string, Symbol>();
            _symbolsets = new Dictionary<string, SymbolSet>();
            _terminals = new Dictionary<string, GrammarElement>();
        }

        public void AddSymbol(string symbolname, Symbol symbol)
        {
            _symbols.Add(symbolname, symbol);
        }
        public void AddSymbolTo(Symbol symbol, string tosymbolset)
        {
            _symbolsets[tosymbolset].Add(symbol);
        }
        public void AddSymbolTo(string symbolname, string tosymbolset)
        {
            if (_symbols.Keys.Contains(symbolname))
                AddSymbolTo(_symbols[symbolname], tosymbolset);
        }

        public void AddSymbolSet(string symbolsetname, SymbolSet symbolset)
        {
            _symbolsets.Add(symbolsetname, symbolset);
        }
        public void AddSymbolSetTo(SymbolSet symbolset, string terminalname)
        {
            _terminals[terminalname].AddElement(symbolset);
        }
        public void AddSymbolSetTo(string symbolsetname, string terminalname)
        {
            if (_symbolsets.Keys.Contains(symbolsetname))
                AddSymbolSetTo(_symbolsets[symbolsetname], terminalname);
        }

        public void AddTerminal(string terminalname, GrammarElement terminal)
        {
            terminal.Name = terminalname;
            _terminals.Add(terminalname, terminal);
        }
        public void AddTerminalTo(string terminal, string toterminal)
        {
            GrammarElement t = _terminals[toterminal];
            Type ttype = t.GetType();
            if(ttype == typeof(CompoundGrammarElement) || ttype == typeof(MultiParseGrammarElement))
            {
                CompoundGrammarElement cge = (CompoundGrammarElement)t;
                cge.AddTerminal(_terminals[terminal]);
                return;
            }
            throw new InvalidOperationException(terminal + " cannot be added to the 'toterminal' parameter " + toterminal + " of " + t.GetType() + " type, and does not support terminal addition.");
        }

        public List<GrammarScriptItem> GetItems(string name, bool includeMatches = false)
        {
            if (!includeMatches)
            {
                List<GrammarScriptItem> list = new List<GrammarScriptItem>();
                if (Terminals.ContainsKey(name))
                    list.Add(new GrammarScriptItem(GrammarScriptItemType.Terminal, name));
                else if (Symbols.ContainsKey(name))
                    list.Add(new GrammarScriptItem(GrammarScriptItemType.Symbol, name));
                else if (SymbolSets.ContainsKey(name))
                    list.Add(new GrammarScriptItem(GrammarScriptItemType.SymbolSet, name));
                return list;
            }
            else
            {
                List<GrammarScriptItem> list = new List<GrammarScriptItem>();
                foreach (string k in Terminals.Keys)
                {
                        if(k.Contains(name))
                            list.Add(new GrammarScriptItem(GrammarScriptItemType.Terminal, k));
                }
                foreach (string k in Symbols.Keys)
                {
                    if (k.Contains(name))
                        list.Add(new GrammarScriptItem(GrammarScriptItemType.Symbol, k));
                }
                foreach (string k in Terminals.Keys)
                {
                    if (k.Contains(name))
                        list.Add(new GrammarScriptItem(GrammarScriptItemType.Terminal, k));
                }
                return list;
            }
        }

        public TestResult<bool> TryParse(ref string input, bool consume)
        {
            TestResult<bool> result = _terminals[Start].Validate(ref input, consume);
            if (input.Length != 0 && consume)
            {
                result.Result = false;
                result.Type = TestResultType.Partial;
                result.Data.Clear();
            }
            return result;
        }
        public TestResult<bool> Parse(string input)
        {
            string s = input;
            TestResult<bool> tryparse = TryParse(ref s, true);
            if (s.Length == 0)
                return tryparse;
            else
                return new TestResult<bool>(false, TestResultType.Failed);
        }

        public GrammarTerminalMap GenerateTerminalMap()
        {
            int totalMappings = Terminals.Count;
            Dictionary<string, int> mappings = new Dictionary<string, int>(totalMappings);
            List<string> keys = Terminals.Keys.ToList<string>();
            GrammarTerminalMap map = new GrammarTerminalMap();
            map.Add("__SYSTEM_GRAMMAR_LGE", 0);
            map.Add("__SYSTEM_GRAMMAR_VARGE", 1);
            map.Add("__SYSTEM_GRAMMAR_CGE", 2);
            map.Add("__SYSTEM_GRAMMAR_MPGE", 3);
            map.Add("__SYSTEM_GRAMMAR_REPGE", 4);
            int count = 5;
            foreach (string key in Terminals.Keys)
            {
                map.Add(key, count++);
            }
            return map;
        }
    }
}

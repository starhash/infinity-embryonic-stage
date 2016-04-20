using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Infinity.Scripting.Grammar;
using System.IO;

namespace Infinity.Scripting
{
    public class InfinityGrammarScript
    {
        private static List<string> _temp = new List<string>();
        public static Grammar.Grammar LoadGrammarFile(string path)
        {
            try
            {
                string input = ((TextReader)new StreamReader(path)).ReadToEnd();
                Grammar.Grammar grammar = LoadGrammar(input);
                if (grammar.Name == null || grammar.Name == "")
                    grammar.Name = new FileInfo(path).Name;
                return grammar;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n");
                foreach (object o in e.Data.Keys)
                {
                    Console.WriteLine("\t" + o + "\n\t\t" + e.Data[o]);
                }
                Console.WriteLine(e.Source);
            }
            return null;
        }

        public static Grammar.Grammar LoadGrammar(string text)
        {
            Grammar.Grammar grammar = new Grammar.Grammar();
            string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            List<InfinityGrammarScriptParseError> errorLines = new List<InfinityGrammarScriptParseError>();
            foreach (string linetemp in lines)
            {
                try
                {
                    string line = linetemp.Trim();
                    ParseAndPutIntoGrammar(grammar, line);
                }
                catch (InfinityGrammarScriptParseError error)
                {
                    errorLines.Add(error);
                    continue;
                }
            }
            if (errorLines.Count != 0)
                throw new InfinityGrammarScriptParseError(errorLines);
            return grammar;
        }

        protected static void ParseAndPutIntoGrammar(Grammar.Grammar grammar, string line)
        {
            if (line.StartsWith("Symbol "))
            {
                ParseSymbol(ref grammar, line);
            }
            else if (line.StartsWith("SymbolRange "))
            {
                ParseSymbolRange(ref grammar, line);
            }
            else if (line.StartsWith("SymbolSet "))
            {
                ParseSymbolSet(ref grammar, line);
            }
            else if (line.StartsWith("GrammarElement "))
            {
                ParseGrammarElement(ref grammar, line);
            }
            else if (line.StartsWith("MultiGrammarElement "))
            {
                ParseMPGE(grammar, line);
            }
            else if (line.StartsWith("Add "))
            {
                AddTerminalTo(ref grammar, line);
            }
            else if (line.StartsWith("StartSymbol"))
            {
                string start = line.Substring(line.IndexOf("StartSymbol") + 11).Trim();
                start = start.Replace("=", "");
                start = start.Trim();
                grammar.Start = start;
            }
            else if (line.StartsWith("Name "))
            {
                string name = line.Substring(4).Trim().Trim('=').Trim();
                grammar.Name = name;
            }
        }

        protected static void ParseSymbol(ref Grammar.Grammar grammar, string line)
        {
            string backup = line;
            LiteralGrammarElement SymbolKeyword = new LiteralGrammarElement("Symbol ");
            if (SymbolKeyword.Validate(ref line, true).Result)
            {
                line = line.Trim();
                string templine = line;
                SymbolSet AlphabetWithSpace = SymbolSet.FromType(SymbolSetPredefinedType.AlphaNumeric);
                AlphabetWithSpace.Add(new Symbol('_'));
                AlphabetWithSpace.Add(new Symbol('$'));
                VariableLengthGrammarElement SymbolName = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, AlphabetWithSpace);
                if (SymbolName.Validate(ref line, true).Result)
                {
                    string name = templine.Substring(0, ((line.Length != 0)?templine.IndexOf(line):templine.Length)).Trim();
                    line = line.Trim();
                    LiteralGrammarElement EqualsSign = new LiteralGrammarElement("=");
                    if (EqualsSign.Validate(ref line, true).Result)
                    {
                        line = line.Trim();
                        try
                        {
                            Symbol symbol = new Symbol(GetSymbolValue(line));
                            grammar.AddSymbol(name, symbol);
                        }
                        catch(InfinityGrammarScriptParseError ie)
                        {
                            throw new InfinityGrammarScriptParseError("Illegal argument specified as symbol, (enclose in ' ' or \" \")", backup, ie);
                        }
                    }
                    else
                    {
                        throw new InfinityGrammarScriptParseError("Syntax requires a symbol value assignment, which is missing",backup);
                    }
                }
                else
                {
                    throw new InfinityGrammarScriptParseError("The Symbol name specified was not valid. Names given can have Letters, Numbers and $ and _ (underscore)", backup);
                }
            }
            else
            {
                throw new InfinityGrammarScriptParseError("Not a Symbol definition", backup);
            }
        }
        protected static void ParseSymbolRange(ref Grammar.Grammar grammar, string line)
        {
            string backup = line;
            LiteralGrammarElement SymbolRangeKeyword = new LiteralGrammarElement("SymbolRange ");
            if (SymbolRangeKeyword.Validate(ref line, true).Result)
            {
                line = line.Trim();
                string templine = line;
                SymbolSet AlphabetWithSpace = SymbolSet.FromType(SymbolSetPredefinedType.AlphaNumeric);
                AlphabetWithSpace.Add(new Symbol('_'));
                AlphabetWithSpace.Add(new Symbol('$'));
                VariableLengthGrammarElement SymbolName = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, AlphabetWithSpace);
                if (SymbolName.Validate(ref line, true).Result)
                {
                    string name = templine.Substring(0, templine.IndexOf(line)).Trim();
                    line = line.Trim();
                    LiteralGrammarElement EqualsSign = new LiteralGrammarElement("=");
                    if (EqualsSign.Validate(ref line, true).Result)
                    {
                        line = line.Trim();
                        string templine2 = line;
                        LiteralGrammarElement FromKeyword = new LiteralGrammarElement("From ");
                        LiteralGrammarElement FromStrKeyword = new LiteralGrammarElement("FromStr ");
                        if (FromKeyword.Validate(ref line, true).Result)
                        {
                            line = line.Trim();
                            string from = line.Substring(0, line.IndexOf("To") - 1).Trim();
                            string to = line.Substring(line.IndexOf("To") + 2, line.Length - line.IndexOf("To") - 2).Trim();
                            from = GetSymbolValue(from);
                            to = GetSymbolValue(to);
                            Console.WriteLine(from + " to " + to);
                            SymbolRange sr = new SymbolRange(from, to);
                            grammar.AddSymbol(name, sr);
                        }
                        else if(FromStrKeyword.Validate(ref line, true).Result)
                        {
                            line = line.Trim();
                            string from = line.Substring(0, line.IndexOf("To") - 1).Trim();
                            string to = line.Substring(line.IndexOf("To") + 2, line.Length - line.IndexOf("To") - 2).Trim();
                            from = GetSymbolValue(from);
                            to = GetSymbolValue(to);
                            Console.WriteLine(from + " to " + to);
                            SymbolRange sr = new SymbolRange(from, to);
                            grammar.AddSymbol(name, sr);
                        }
                        else
                        {
                            throw new InfinityGrammarScriptParseError("From ranges specified by wrong keyword or syntax error.", backup);
                        }
                    }
                    else
                    {
                        throw new InfinityGrammarScriptParseError("Syntax requires a symbol value assignment, which is missing.", backup);
                    }
                }
                else
                {
                    throw new InfinityGrammarScriptParseError("The Symbol name specified was not valid. Names given can have Letters, Numbers and $ and _.", backup);
                }
            }
            else
            {
                throw new InfinityGrammarScriptParseError("Not a valid SymbolRange definition.", backup);
            }
        }
        protected static string GetSymbolValue(string raw)
        {
            if (raw.StartsWith("'") && raw.EndsWith("'"))
                return raw.Substring(1, raw.Length - 2);
            else if(raw.StartsWith("\"") && raw.EndsWith("\""))
                return raw.Substring(1, raw.Length - 2);
            else if (raw.StartsWith("ux"))
            {
                string temp = raw.Substring(2);
                int rawi;
                bool tre = int.TryParse(temp, System.Globalization.NumberStyles.HexNumber, CultureInfo.CurrentCulture, out rawi);
                if (tre)
                    return (char)rawi + "";
                else throw new InfinityGrammarScriptParseError("Invalid symbol value specified for a range : " + raw, raw);
            }
            else if (raw.StartsWith("u"))
            {
                string temp = raw.Substring(1);
                int rawi;
                bool tre = int.TryParse(temp, System.Globalization.NumberStyles.Integer, CultureInfo.CurrentCulture, out rawi);
                if(tre)
                    return (char)rawi + "";
                else throw new InfinityGrammarScriptParseError("Invalid symbol value specified for a range : " + raw, raw);
            }
            else
            {
                throw new InfinityGrammarScriptParseError("Invalid symbol value specified for a range : " + raw, raw);
            }
        }
        protected static void ParseSymbolSet(ref Grammar.Grammar grammar, string line)
        {
            string backup = line;
            LiteralGrammarElement SymbolKeyword = new LiteralGrammarElement("SymbolSet ") { Name = "SymbolSetKeyword"};
            if (SymbolKeyword.Validate(ref line, true).Result)
            {
                line = line.Trim();
                string exception = "";
                if (line.Contains("except"))
                {
                    int idxx = line.IndexOf("except");
                    exception = line.Substring(idxx + 7).Trim();
                    line = line.Substring(0, idxx);
                }
                string templine = line;
                SymbolSet Identifier = SymbolSet.FromType(SymbolSetPredefinedType.AlphaNumeric);
                Identifier.Add(new Symbol('_'));
                Identifier.Add(new Symbol('$'));
                VariableLengthGrammarElement SymbolName = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, Identifier) { Name = "Identifier" };
                if (SymbolName.Validate(ref line, true).Result)
                {
                    string name = templine.Substring(0, templine.IndexOf(line)).Trim();
                    line = line.Trim();
                    LiteralGrammarElement EqualsSign = new LiteralGrammarElement("=") {Name = "EqualsSign"};
                    if (EqualsSign.Validate(ref line, true).Result)
                    {
                        line = line.Trim();
                        #region Symbol Set %[...]
                        if (line.StartsWith("%"))
                        {
                            line = line.Substring(1);
                            line = line.Trim();
                            if (line.StartsWith("[") && line.EndsWith("]"))
                            {
                                line = line.Substring(1, line.Length - 2);
                                SymbolSet set = new SymbolSet();
                                foreach (char c in line)
                                {
                                    set.Add(new Symbol(c));
                                }
                                foreach (char c in exception)
                                {
                                    set.RemoveSymbol(new Symbol(c));
                                }
                                grammar.AddSymbolSet(name, set);
                                LiteralGrammarElement none = new LiteralGrammarElement();
                                none.AddElement(set);
                                VariableLengthGrammarElement star = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Star, set);
                                VariableLengthGrammarElement plus = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, set);
                                grammar.AddTerminal(name, none);
                                grammar.AddTerminal(name + "*", star);
                                grammar.AddTerminal(name + "+", plus);
                            }
                        }
                        #endregion
                        #region Symbol Set [...]
                        else if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            line = line.Substring(1, line.Length - 2);
                            SymbolSet set = GetSetValues(line);
                            foreach (char c in exception)
                            {
                                set.RemoveSymbol(new Symbol(c));
                            }
                            grammar.AddSymbolSet(name, set);
                            LiteralGrammarElement none = new LiteralGrammarElement();
                            none.AddElement(set);
                            VariableLengthGrammarElement star = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Star, set);
                            VariableLengthGrammarElement plus = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, set);
                            grammar.AddTerminal(name, none);
                            grammar.AddTerminal(name + "*", star);
                            grammar.AddTerminal(name + "+", plus);
                        }
                        #endregion
                        #region Symbol Set with Predefined/Predeclared Sets
                        else
                        {
                            string[] types = Enum.GetNames(typeof(SymbolSetPredefinedType));
                            LiteralGrammarElement NewKeyword = new LiteralGrammarElement("new");
                            LiteralGrammarElement SetTypes = new LiteralGrammarElement(types);
                            CompoundGrammarElement PreDefinedSet = new CompoundGrammarElement(NewKeyword, SetTypes) { Delimiters = " \n\r" };
                            string[] values = line.Split(',');
                            SymbolSet set = new SymbolSet();
                            foreach (string valueq in values)
                            {
                                string value = valueq.Trim();
                                string iv = value;
                                string iv2 = value;
                                if (PreDefinedSet.Validate(ref iv2, true).Result)
                                {
                                    NewKeyword.Validate(ref value, true);
                                    SymbolSetPredefinedType type = (SymbolSetPredefinedType)Enum.Parse(typeof(SymbolSetPredefinedType), value);
                                    SymbolSet temp = SymbolSet.FromType(type);
                                    foreach (Symbol s in temp)
                                    {
                                        set.Add(s);
                                    }
                                }
                                else if (SymbolName.Validate(ref iv, true).Result)
                                {
                                    if (grammar.Symbols.Keys.Contains(value))
                                    {
                                        set.Add(grammar.Symbols[value]);
                                    }
                                    else if (grammar.SymbolSets.Keys.Contains(value))
                                    {
                                        foreach (Symbol sym in grammar.SymbolSets[value])
                                        {
                                            set.Add(sym);
                                        }
                                    }
                                    else
                                    {
                                        throw new InfinityGrammarScriptParseError("Symbol not declared or found - " + value, backup);
                                    }
                                }
                                else
                                {
                                    throw new InfinityGrammarScriptParseError("Invalid parameters passed - " + value, backup);
                                }
                            }
                            foreach (char c in exception)
                            {
                                set.RemoveSymbol(new Symbol(c));
                            }
                            grammar.SymbolSets.Add(name, set);
                            LiteralGrammarElement none = new LiteralGrammarElement();
                            none.AddElement(set);
                            VariableLengthGrammarElement star = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Star, set);
                            VariableLengthGrammarElement plus = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, set);
                            grammar.AddTerminal(name, none);
                            grammar.AddTerminal(name + "*", star);
                            grammar.AddTerminal(name + "+", plus);
                        }
                        #endregion  
                    }
                    else
                    {
                        throw new InfinityGrammarScriptParseError("Syntax requires a symbol value assignment, which is missing.", backup);
                    }
                }
                else
                {
                    throw new InfinityGrammarScriptParseError("The Symbol name specified was not valid. Names given can have Letters, Numbers and $ and _.", backup);
                }
            }
            else
            {
                throw new InfinityGrammarScriptParseError("Not a SymbolSet definition.", backup);
            }
        }
        protected static SymbolSet GetSetValues(string values)
        {
            SymbolSet set = new SymbolSet();
            string[] valuess = values.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in valuess)
            {
                set.Add(new Symbol(GetSymbolValue(s.Trim())));
            }
            return set;
        }
        protected static void ParseGrammarElement(ref Grammar.Grammar grammar, string line)
        {
            string backup = line;
            LiteralGrammarElement SymbolKeyword = new LiteralGrammarElement("GrammarElement ");
            if (SymbolKeyword.Validate(ref line, true).Result)
            {
                line = line.Trim();
                string templine = line;
                SymbolSet AlphabetWithSpace = SymbolSet.FromType(SymbolSetPredefinedType.AlphaNumeric);
                AlphabetWithSpace.Add(new Symbol('_'));
                AlphabetWithSpace.Add(new Symbol('$'));
                AlphabetWithSpace.Add(new Symbol('/'));
                VariableLengthGrammarElement SymbolName = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, AlphabetWithSpace);
                if (SymbolName.Validate(ref line, true).Result)
                {
                    string name = templine.Substring(0, templine.IndexOf(line)).Trim();
                    line = line.Trim();
                    LiteralGrammarElement EqualsSign = new LiteralGrammarElement("=");
                    if (EqualsSign.Validate(ref line, true).Result)
                    {
                        line = line.Trim();
                        if (line.Contains(" or ") || line.Contains(" | ") || (line.Contains("precedence")
                            && (line.Contains("Ascending") || line.Contains("Descending"))))
                        {
                            MultiParsePrecedenceType precedence = MultiParsePrecedenceType.Normal;
                            if (line.IndexOf(',') >= 0)
                            {
                                string precedenceString = line.Substring(line.IndexOf(',')+1).Trim();
                                line = line.Substring(0, line.IndexOf(',')).Trim();
                                LiteralGrammarElement precKey = new LiteralGrammarElement("precedence");
                                LiteralGrammarElement colon = new LiteralGrammarElement(":");
                                if (precKey.Validate(ref precedenceString, true).Result)
                                {
                                    precedenceString = precedenceString.Trim();
                                    if (colon.Validate(ref precedenceString, true).Result)
                                    {
                                        string[] precedenceList = Enum.GetNames(typeof(MultiParsePrecedenceType));
                                        LiteralGrammarElement type = new LiteralGrammarElement(precedenceList);
                                        precedenceString = precedenceString.Trim();
                                        if(type.Validate(ref precedenceString, false).Result)
                                        {
                                            precedence = (MultiParsePrecedenceType)Enum.Parse(typeof(MultiParsePrecedenceType), precedenceString);
                                        }
                                    }
                                }
                            }
                            string[] terminals = line.Split(new string[] { " or ", " | " }, StringSplitOptions.RemoveEmptyEntries);
                            MultiParseGrammarElement mpge = new MultiParseGrammarElement();
                            mpge.Precedence = precedence;
                            int count = 0;
                            foreach (string ss in terminals)
                            {
                                try
                                {
                                    GrammarElement cge = ParseCGE(grammar, ss);
                                    if (cge.Name == null || cge.Name == "")
                                        cge.Name = name + ":" + count++;
                                    mpge.AddTerminal(cge);
                                }
                                catch (KeyNotFoundException k)
                                {
                                    _temp.Add(backup);
                                    throw new InfinityGrammarScriptParseError("No such terminal to add to. " + k.Message, backup);
                                }
                            }
                            grammar.AddTerminal(name, mpge);
                        }
                        else
                        {
                            string delimiters = " \n\r\t";
                            int idx = line.LastIndexOf(',');
                            if (line.Substring(idx + 1).Trim().StartsWith("delimiters"))
                            {
                                delimiters = line.Substring(idx + 1).Trim();
                                line = line.Substring(0, idx);
                                LiteralGrammarElement lge = new LiteralGrammarElement("delimiters");
                                if (lge.Validate(ref delimiters, true).Result)
                                {
                                    delimiters = delimiters.Trim();
                                    if (delimiters.StartsWith("[") && delimiters.EndsWith("]"))
                                    {
                                        delimiters = delimiters.Substring(1, delimiters.Length - 2);
                                        if (delimiters.Length == 0)
                                            delimiters = "@None";
                                    }
                                }
                                else
                                {
                                    throw new InfinityGrammarScriptParseError("Delimiters not declared properly - " + delimiters, backup);
                                }
                            }
                            try
                            {
                                GrammarElement cge = ParseCGE(grammar, line, delimiters);
                                grammar.AddTerminal(name, cge);
                            }
                            catch (KeyNotFoundException)
                            {
                                _temp.Add(backup);
                                throw new InfinityGrammarScriptParseError("No such terminal to add to.", backup);
                            }
                        }
                    }
                }
            }
        }
        protected static GrammarElement ParseCGE(Grammar.Grammar grammar, string line, string delimiters = "@Default")
        {
            CompoundGrammarElement cge = new CompoundGrammarElement();
            if (delimiters.Equals("@Default"))
            {
                cge.Delimiters = " \n\r\t";
            }
            else if (delimiters.Equals("@None"))
            {
                cge.Delimiters = "";
            }
            else
            {
                cge.Delimiters = delimiters;
            }
            string[] tokens = line.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 1 && grammar.Terminals.ContainsKey(tokens[0]))
            {
                return grammar.Terminals[tokens[0]];
            }
            foreach (string tokenw in tokens)
            {
                string token = tokenw.Trim();
                if ((token.StartsWith("'") && token.EndsWith("'")) || (token.StartsWith("\"") && token.EndsWith("\""))
                    || token.StartsWith("ux") || token.StartsWith("u"))
                {
                    string value = GetSymbolValue(token);
                    LiteralGrammarElement lge = new LiteralGrammarElement(value);
                    cge.AddTerminal(lge);
                }
                else if (grammar.Terminals.ContainsKey(token))
                {
                    cge.AddTerminal(grammar.Terminals[token]);
                }
                else if (token.EndsWith("*"))
                {
                    string tok = token.Substring(0, token.Length - 1);
                    if (grammar.Terminals.ContainsKey(tok))
                    {
                        RepetitiveGrammarElement rge = new RepetitiveGrammarElement(grammar.Terminals[tok]);
                        cge.AddTerminal(rge);
                    }
                }
                else if (!grammar.Terminals.ContainsKey(token))
                {
                    throw new KeyNotFoundException();
                }
            }
            return cge;
        }
        protected static void ParseMPGE(Grammar.Grammar grammar, string line)
        {
            string backup = line;
            LiteralGrammarElement SymbolKeyword = new LiteralGrammarElement("MultiGrammarElement ");
            if (SymbolKeyword.Validate(ref line, true).Result)
            {
                line = line.Trim();
                string templine = line;
                SymbolSet AlphabetWithSpace = SymbolSet.FromType(SymbolSetPredefinedType.AlphaNumeric);
                AlphabetWithSpace.Add(new Symbol('_'));
                AlphabetWithSpace.Add(new Symbol('$'));
                AlphabetWithSpace.Add(new Symbol('/'));
                VariableLengthGrammarElement SymbolName = new VariableLengthGrammarElement(VariableLengthGrammarElementType.Plus, AlphabetWithSpace);
                if (SymbolName.Validate(ref line, true).Result)
                {
                    string name = templine.Substring(0, templine.IndexOf(line)).Trim();
                    line = line.Trim();
                    LiteralGrammarElement EqualsSign = new LiteralGrammarElement("=");
                    if (EqualsSign.Validate(ref line, true).Result)
                    {
                        line = line.Trim();
                        MultiParsePrecedenceType precedence = MultiParsePrecedenceType.Normal;
                        if (line.LastIndexOf(',') >= 0)
                        {
                            string precedenceString = line.Substring(line.IndexOf(',') + 1).Trim();
                            line = line.Substring(0, line.IndexOf(',')).Trim();
                            LiteralGrammarElement precKey = new LiteralGrammarElement("precedence");
                            LiteralGrammarElement colon = new LiteralGrammarElement(":");
                            if (precKey.Validate(ref precedenceString, true).Result)
                            {
                                precedenceString = precedenceString.Trim();
                                if (colon.Validate(ref precedenceString, true).Result)
                                {
                                    string[] precedenceList = Enum.GetNames(typeof(MultiParsePrecedenceType));
                                    LiteralGrammarElement type = new LiteralGrammarElement(precedenceList);
                                    precedenceString = precedenceString.Trim();
                                    if (type.Validate(ref precedenceString, false).Result)
                                    {
                                        precedence = (MultiParsePrecedenceType)Enum.Parse(typeof(MultiParsePrecedenceType), precedenceString);
                                    }
                                    else throw new InfinityGrammarScriptParseError("Error", backup);
                                }
                                else throw new InfinityGrammarScriptParseError("Precedence is always followed by a Colon \":\"", backup);
                            }
                            else throw new InfinityGrammarScriptParseError("Invalid syntax. Only 'precedence' keyword allowed here", backup);
                        }
                        string[] terminals = line.Split(new string[] { " or ", " | " }, StringSplitOptions.RemoveEmptyEntries);
                        MultiParseGrammarElement mpge = new MultiParseGrammarElement();
                        mpge.Precedence = precedence;
                        int count = 0;
                        foreach (string ss in terminals)
                        {
                            try
                            {
                                GrammarElement cge = ParseCGE(grammar, ss);
                                if (cge.Name == null || cge.Name == "")
                                    cge.Name = name + ":" + count++;
                                mpge.AddTerminal(cge);
                            }
                            catch (KeyNotFoundException)
                            {
                                _temp.Add(backup);
                                throw new InfinityGrammarScriptParseError("No such terminal to add to.", backup);
                            }
                        }
                        grammar.AddTerminal(name, mpge);
                    }
                    else throw new InfinityGrammarScriptParseError("MultiGrammarElement requires assignment after the name, missing '=' sign", backup);
                }
                else throw new InfinityGrammarScriptParseError("Check name", backup);
            }
            else throw new InfinityGrammarScriptParseError("Undetected error", backup);
        }
        protected static void AddTerminalTo(ref Grammar.Grammar grammar, string line)
        {
            string _line = line;
            LiteralGrammarElement addKeyword = new LiteralGrammarElement("Add");
            if (addKeyword.Validate(ref _line, true).Result)
            {
                if (_line.IndexOf(" To ") != -1)
                {
                    string to = _line.Substring(_line.IndexOf("To") + 2).Trim();
                    if (!grammar.Terminals.ContainsKey(to))
                        throw new InfinityGrammarScriptParseError("Terminal not found to perform addition of terminals - " + to, _line);
                    _line = _line.Substring(0, _line.IndexOf("To")).Trim();
                    string[] _terminals = _line.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string _terminal in _terminals)
                    {
                        if (grammar.Terminals.ContainsKey(_terminal))
                            grammar.AddTerminalTo(_terminal, to);
                        else
                            throw new InfinityGrammarScriptParseError("No such terminal to add to - " + to + " : " + _terminal, _line);
                    }
                }
                else
                {
                    throw new InfinityGrammarScriptParseError("Add statement improperly formed. Missing 'To' keyword.", _line);
                }
            }
            else
            {
                throw new InfinityGrammarScriptParseError("Please check whether you have typed the correct syntax.", _line);
            }
        }
    }

    public class InfinityGrammarScriptExtension : InfinityGrammarScript
    {
        public static void ParseAndPutIntoGrammar(Grammar.Grammar grammar, string line)
        {
            InfinityGrammarScript.ParseAndPutIntoGrammar(grammar, line);
        }
    }
}

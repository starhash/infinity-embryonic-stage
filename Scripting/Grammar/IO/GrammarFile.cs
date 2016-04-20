using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Infinity.Scripting.Utils;
using Infinity.Scripting.Grammar.Parsing;

namespace Infinity.Scripting.Grammar.IO
{
    public class GrammarFile
    {
        private FileInfo _gfile;
        private Grammar _grammar;
        public FileInfo FileInfo { get { return _gfile; } }
        public Grammar Grammar { get { return _grammar; } }

        public GrammarFile(string path)
        {
            try
            {
                _gfile = new FileInfo(path);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine(fnfe + "\n\t" + fnfe.Message);
            }
        }

        public void LoadGrammar()
        {
            try
            {
                _grammar = InfinityGrammarScript.LoadGrammarFile(_gfile.FullName);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception + "\n\t" + exception.Message);
            }
        }
        public static string[] LoadAndTest(string grammarfile, string testfile, bool generateParseResult = true)
        {
            try
            {
                string[] retval = new string[2];
                string igptfile = "";
                string igmlfile = "";
                FileInfo fi = new FileInfo(testfile);
                TextReader tr = new StreamReader(testfile);
                Grammar g = InfinityGrammarScript.LoadGrammarFile(grammarfile);
                if (g != null)
                {
                    Console.WriteLine("Grammar Loaded - " + g.Name + "\n\tfrom - " + grammarfile);
                    Console.WriteLine("Reading input file - " + testfile);
                    string input = tr.ReadToEnd();
                    Console.WriteLine("Finished reading.");
                    Console.WriteLine("Testing file against grammar...");
                    TestResult<bool> result = g.TryParse(ref input, true);
                    if (generateParseResult)
                    {
                        if (result.Data.ContainsKey("$PARSETREE.NODE$"))
                        {
                            ParseTreeNode ptn = (ParseTreeNode)result.Data["$PARSETREE.NODE$"];
                            string finame = fi.Name;
                            if (finame.LastIndexOf(".") != 0)
                            {
                                finame = finame.Remove(finame.LastIndexOf("."));
                            }
                            string name = fi.DirectoryName + "\\" + finame;
                            Console.WriteLine(name);
                            igptfile = name + ".igpt";
                            igmlfile = name + ".igml";
                            TextWriter tw = new StreamWriter(igptfile);
                            tw.Write(ptn.ToTextualRepresentation().Trim());
                            tw.Close();
                            Console.WriteLine("InfinityGrammarParseTree File generated.");
                            tw = new StreamWriter(igmlfile);
                            tw.Write(ptn.ToXMLTextRepresentation().Trim());
                            tw.Close();
                            Console.WriteLine("InfinityGrammarMarkupLanguage File generated.");
                            List<string> elements = new List<string>();
                            foreach (GrammarElement ge in g.Terminals.Values)
                            {
                                if (!elements.Contains(ge.Name))
                                    elements.Add(ge.Name);
                            }
                            CreateEnumCode(g.Name, name, elements.ToArray());
                        }
                    }
                    if (result.Result)
                        Console.WriteLine("File contents accepted successfully!");
                    else
                        Console.WriteLine("File contents failed to satisfy grammar.");
                    Console.WriteLine("\nTest Result Details - \n" + result);
                    Console.WriteLine("Input string after processing - " + input);
                    if(!(igmlfile.Length == 0 && igptfile.Length == 0)) {
                        retval[0] = igptfile;
                        retval[1] = igmlfile;
                        return retval;
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("Specified grammar file "
                    + grammarfile + " or specified test file " + testfile + "does not exist");
            }
            return null;
        }

        private static void CreateEnumCode(string name, string filename, string[] terminals)
        {
            try
            {
                name = name.Replace(' ', '_').Replace('.', '_');
                string enumcode = "public enum " + name + "\n{";
                enumcode += "\n\t__SYSTEM_GRAMMAR_CMPGE,";
                enumcode += "\n\t__SYSTEM_GRAMMAR_LGE,";
                enumcode += "\n\t__SYSTEM_GRAMMAR_MPGE,";
                enumcode += "\n\t__SYSTEM_GRAMMAR_REPGE,";
                enumcode += "\n\t__SYSTEM_GRAMMAR_VARGE";
                foreach (string s in terminals)
                {
                    enumcode += ",\n\t" + s.Replace("*", "_Star").Replace("+", "_Plus");
                }
                enumcode += "\n}";
                TextWriter tw = new StreamWriter(filename + ".cs");
                tw.Write(enumcode);
                tw.Close();
            }
            catch (Exception) { }
        }
    }
}

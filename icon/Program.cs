using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infinity.Scripting.Grammar.IO;
using System.Xml;
using System.IO;
using Infinity.Scripting;
using Infinity.Engine;

namespace icon
{
    class Program
    {
        static void Main(string[] args)
        {
            string gfile = @"F:\Infinity (I#)\basictest.igrm";
            string tfile = @"F:\Infinity (I#)\basictest.is";
            string[] resultfiles = GrammarFile.LoadAndTest(gfile, tfile);
            XmlDocument igml = new XmlDocument();
            TextReader tr = new StreamReader(resultfiles[1]);
            igml.LoadXml(tr.ReadToEnd());
            tr.Close();
            Interpreter InfinityInterpreter = new InfinityInterpreter();
            Scope defaultScope = new Scope();
            foreach (XmlElement xe in igml)
            {
                InfinityInterpreter.Interpret(xe, defaultScope);
            }
            RuntimeEngine.VariablePool.GetHashCode();
            Console.ReadKey();
        }

        

        public static void PrintHelp()
        {

        }
    }
}

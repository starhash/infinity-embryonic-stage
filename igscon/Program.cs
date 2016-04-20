using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Infinity.Scripting;
using Infinity.Scripting.Grammar;
using Infinity.Scripting.Utils;
using Infinity.Scripting.Grammar.Parsing;
using Infinity.Scripting.Grammar.IO;

namespace igscon
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime time0 = DateTime.Now;
            #region Console
            if (args.Length == 0)
            {
                GrammarFile grammarFile = null;
                string temp = "";
                bool loadedtestfile = false;
                string currentDirectory = null;
                string testfilepath = null;
                PrintHelp();
                string command = Console.ReadLine();
                while (!command.Equals("exit"))
                {
                    try
                    {
                        #region SetDirectory
                        string filepath = command.Trim(' ', '\t', '\"');
                        DirectoryInfo di = null;
                        try
                        {
                            di = new DirectoryInfo(currentDirectory + "\\" + filepath);
                            if (!di.Exists)
                                throw new Exception();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                di = new DirectoryInfo(filepath);
                                if (!di.Exists)
                                    throw new Exception();
                            }
                            catch (Exception)
                            {
                                di = null;
                            }
                        }
                        if (di != null)
                            currentDirectory = di.FullName;
                        #endregion
                        #region Load
                        else if (command.Trim().StartsWith("load "))
                        {
                            try
                            {
                                filepath = command.Substring(command.IndexOf("load") + 4).Trim(' ', '\t', '\"');
                                ConsoleKey key = ConsoleKey.Enter;
                                if (grammarFile != null)
                                {
                                    Console.WriteLine("Grammar already loaded from - \n\t" + grammarFile.FileInfo.FullName);
                                    Console.WriteLine("\tPress <Enter> to overwrite loaded grammar.\n\tAny other key to cancel.");
                                    ConsoleKeyInfo cki = Console.ReadKey();
                                    key = cki.Key;
                                }
                                if (key == ConsoleKey.Enter)
                                {
                                    if (filepath.Length != 0)
                                    {
                                        if (filepath[1] != ':')
                                        {
                                            if (new FileInfo(currentDirectory + "\\" + filepath).Exists)
                                            {
                                                grammarFile = new GrammarFile(currentDirectory + "\\" + filepath);
                                            }
                                        }
                                    }
                                    if (grammarFile == null)
                                        grammarFile = new GrammarFile(filepath);
                                    if (grammarFile != null)
                                    {
                                        grammarFile.LoadGrammar();
                                    }
                                    if (grammarFile != null && grammarFile.Grammar != null)
                                        Console.WriteLine("Grammar from " + grammarFile.FileInfo.Name + " successfully loaded.");
                                    else
                                        Console.WriteLine("Grammar load unsuccessful. Check file name/path.");
                                }
                            }
                            catch (IOException e)
                            {
                                Console.Error.WriteLine(e + "\n" + e.Message);
                            }
                        }
                        #endregion
                        #region Test
                        else if (command.Trim().StartsWith("test "))
                        {
                            ConsoleKey key = ConsoleKey.Enter;
                            filepath = command.Substring(command.IndexOf("test") + 4).Trim(' ', '\t', '\"');
                            if (loadedtestfile && testfilepath != null)
                            {
                                Console.WriteLine("Test file already loaded from - \n\t" + testfilepath);
                                Console.WriteLine("\tPress <Enter> to overwrite loaded test file.\n\tAny other key to cancel.");
                                ConsoleKeyInfo cki = Console.ReadKey();
                                key = cki.Key;
                            }
                            if (key == ConsoleKey.Enter)
                            {
                                string filename = null;
                                if (filepath.Length != 0)
                                {
                                    if (filepath[1] != ':')
                                    {
                                        if (new FileInfo(currentDirectory + "\\" + filepath).Exists)
                                        {
                                            filename = currentDirectory + "\\" + filepath;
                                        }
                                    }
                                }
                                if (grammarFile == null)
                                {
                                    Console.WriteLine("Please specify grammar file before testing any file.");
                                }
                                else
                                {
                                    if (filename == null)
                                        filename = command.Substring(command.IndexOf("test") + 4).Trim(' ', '\t', '\"');
                                    if (new FileInfo(filename).Exists)
                                    {
                                        testfilepath = filename;
                                        loadedtestfile = true;
                                        TextReader tr = new StreamReader(testfilepath);
                                        string testtext = tr.ReadToEnd();
                                        tr.Close();
                                        tr.Dispose();
                                        TestResult<bool> testresult = grammarFile.Grammar.TryParse(ref testtext, true);
                                        Console.WriteLine("Grammar test result - " + testresult);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Loading test file from " + testfilepath + " aborted.");
                                testfilepath = null;
                            }
                        }
                        #endregion
                        #region Discarding
                        else if (command.Trim().StartsWith("discard"))
                        {
                            command.Remove(0, 7);
                            if (command.StartsWith("grammar"))
                            {
                                grammarFile = null;
                            }
                            else if (command.StartsWith("test"))
                            {
                                loadedtestfile = false;
                                testfilepath = null;
                            }
                        }
                        #endregion
                        #region Status
                        else if (command.Trim().Equals("status"))
                        {
                            Console.WriteLine("Current IGSConsole status -");
                            if (grammarFile == null)
                                Console.WriteLine("\tGrammar not loaded.");
                            else
                                Console.WriteLine("\tGrammar loaded from - " + grammarFile.FileInfo.FullName);
                            if (!loadedtestfile)
                                Console.WriteLine("\tTest file not loaded.");
                            else
                                Console.WriteLine("\tTest file loaded from - " + testfilepath);
                        }
                        #endregion
                        #region Create
                        else if (command.Trim().Equals("create"))
                        {
                            Console.WriteLine("Entering Grammar creation mode...");
                            int current = temp.Count(x => x == '\n');
                            Console.WriteLine("Current Buffer contains code for "+current + " lines.");
                            Console.WriteLine("\tType a blank line or just press Enter to abort.");
                            string templine = null;
                            do
                            {
                                Console.Write(++current + " ");
                                templine = Console.ReadLine();
                                temp += templine + "\n";
                            }
                            while (templine.Trim().Length != 0);
                        }
                        #endregion
                        #region View
                        else if (command.Trim().StartsWith("view"))
                        {
                            command = command.Trim().Remove(0, 4).Trim();
                            if (command.Length == 0)
                            {
                                Console.WriteLine("view mode");
                                Console.WriteLine("\tgrammar\t - View loaded grammar file.");
                                Console.WriteLine("\ttest\t - View loaded test file.");
                                Console.WriteLine("\tbuffer\t - View current editing buffer grammar script.");
                                command = Console.ReadLine().Trim();
                            }
                            if (command.Equals("buffer"))
                            {
                                string[] lines = temp.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                int outer = lines.Length / 10 + 1;
                                for (int i = 0; i < outer; i++)
                                {
                                    for (int j = 0; j < 10 && (i * 10 + j) < lines.Length; j++)
                                    {
                                        Console.WriteLine(((i * 10 + j) + 1) + " " + lines[(i * 10 + j)]);
                                    }
                                    Console.WriteLine("Press any key to continue...");
                                    Console.ReadKey();
                                }
                            }
                            else if (command.Equals("grammar"))
                            {

                            }
                            else if (command.Equals("test"))
                            {

                            }
                        }
                        #endregion
                        #region Edit
                        else if (command.Trim().StartsWith("edit"))
                        {
                            command = command.Trim().Remove(0, 4).Trim();
                            if (command.StartsWith("buffer"))
                            {
                                string[] lines = temp.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries);
                                bool repeat = true;
                                while (repeat)
                                {
                                    Console.Write("\nEnter line number (1 to " + lines.Length + ") to start edit otherwise to cancel action.");
                                    int repline = 0;
                                    bool p = int.TryParse(Console.ReadLine(), out repline);
                                    if (p && (repline > 0 && repline <= lines.Length))
                                    {
                                        Console.WriteLine("Line " + repline + " is\n\t" + lines[repline - 1] + "\nedit? (<Enter> to edit, else ignore)");
                                        ConsoleKey ck = Console.ReadKey().Key;
                                        if (ck == ConsoleKey.Enter)
                                        {
                                            Console.WriteLine("Enter new line content - ");
                                            string line = Console.ReadLine();
                                            lines[repline - 1] = line;
                                        }
                                    }
                                    else if (!p && !(repline > 0 && repline <= lines.Length))
                                    {
                                        Console.WriteLine("Edit action aborted.");
                                        break;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Please try again.");
                                    }
                                }
                                temp = string.Join("\n", lines);
                            }
                        }
                        #endregion
                        #region Check
                        else if (command.Trim().Equals("check"))
                        {
                            if (temp.Length == 0)
                            {
                                Console.WriteLine("Buffer empty. Switching to auto create mode.");
                                command = "create";
                                continue;
                            }
                            else
                            {
                                Grammar grammar = InfinityGrammarScript.LoadGrammar(temp);
                                if (grammar != null)
                                {
                                    Console.WriteLine("Grammar has no erros.");
                                }
                                else
                                {
                                    Console.WriteLine("Please check grammar for erros.");
                                }
                            }
                        }
                        #endregion
                        #region SaveTo
                        else if (command.Trim().StartsWith("saveto"))
                        {
                            command = command.Trim().Remove(0, 6).Trim();
                            FileInfo f = new FileInfo(command);
                            if (!f.Exists)
                            {
                                try
                                {
                                    FileInfo ff = new FileInfo(currentDirectory + "\\" + command);
                                    f = ff;
                                }
                                catch (Exception) { }
                            }
                            bool append = false;
                            if (f.Exists)
                            {
                                Console.WriteLine("File already exists, overwrite? \n\t(<Enter> else any key)");
                                if (Console.ReadKey().Key == ConsoleKey.Enter)
                                    append = false;
                                else
                                {
                                    Console.WriteLine("Aborting write session.");
                                    continue;
                                }
                            }
                            try
                            {
                                StreamWriter sw = new StreamWriter(command, append);
                                sw.Write(temp);
                                Console.WriteLine("Saved to " + command);
                                sw.Close();
                            }
                            catch (Exception) { }
                        }
                        #endregion
                    }
                    catch (Exception) { }
                    if (currentDirectory != null)
                        Console.Write("\nigscon @ " + new DirectoryInfo(currentDirectory).FullName + ">");
                    else
                        Console.Write("\nigscon>");
                    command = Console.ReadLine();
                }
            }
            #endregion
            #region Test
            else if (args.Length >= 1 && args[0].Equals("-test"))
            {
                GrammarFile.LoadAndTest(args[1], args[2]);
            }
            #endregion
            #region Help
            else if (args.Length >= 1 && args[0].Equals("-help"))
            {
                Console.WriteLine("Infinity Grammar Script Console - Version 1.0.0.1_1");
                Console.WriteLine("Usage - \n\tigscon\n\t\t- Start the IGSConsole\n\tigscon -test <grammarfile> <testfile>"
                    + "\n\t\t- to test a file to the grammar defined in the grammar file"
                    + "\n\tigscon -help\n\t\t- to bring up this again");
            }
        
            #endregion
            DateTime time1 = DateTime.Now;
            TimeSpan difference = TimeSpan.FromTicks(time1.Subtract(time0).Ticks);
            Console.WriteLine("Console Ran for " + difference.TotalMilliseconds + "ms");
        }

        private static void PrintHelp()
        {
            Console.WriteLine("igscon>\nInfinity Grammar Script Console - Version 1.0.0.1_1");
            Console.WriteLine("Console Actions - ");
            Console.WriteLine("\tload <file>\t - Load Grammar File from specified path.");
            Console.WriteLine("\ttest <file>\t - Load and test the specified file\n\t\t\t    with the loaded grammar.");
            Console.WriteLine("\tdiscard grammar\t - Discard currently loaded grammar.");
            Console.WriteLine("\tdiscard test\t - Discard currently loaded test file.");
            Console.WriteLine("\tcreate\t\t - Create a new grammar.");
            Console.WriteLine("\tview grammar\t - View loaded grammar file.");
            Console.WriteLine("\tview test\t - View loaded test file.");
            Console.WriteLine("\tview buffer\t - View currently editing buffer grammar script.");
            Console.WriteLine("\tedit buffer\t - Replace line in the currently editing\n\t\t\t    buffer grammar script.");
            Console.WriteLine("\tcheck\t\t - Check grammar for syntax validity.");
            Console.WriteLine("\tsaveto <file>\t - Save file to specified path.");
            Console.WriteLine("\texit\t\t - Exit from console.");
            Console.Write("\n\nigscon> ");
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Infinity.Scripting.Grammar;
using Infinity.Scripting.Grammar.Parsing;

namespace InfinityGrammarIDE
{
    public partial class Form1 : Form
    {
        Grammar parsedGrammar = null;
        Color editorBackColor = Color.FromArgb(255, 253, 246, 227);
        Color editorForeColor = Color.FromArgb(255, 0, 0, 0);
        public class TextRange
        {
            public int Start { get; set; }
            public int Length { get; set; }
            public TextRange(int start, int length)
            {
                Start = start;
                Length = length;
            }
        }
        private List<TextRange> _errorRanges;
        #region Highlighting Definitions
        public string[][] keyword_sets = {
                                             new string[]{ "Symbol", "SymbolRange", "SymbolSet", "GrammarElement", "MultiGrammarElement" },
                                             new string[]{ "new", "From", "FromStr", "To", "Add", "except", "precedence", "delimiters" },
                                             new string[]{ "StartSymbol", "Name" },
                                             new string[]{ "Ascending", "Descending", "Normal", "Custom" },
                                             new string[]{ "=", "or", "|", "%" }
                                         };
        public Color[] keywordForeColors = { Color.FromArgb(0, 128, 255), Color.FromArgb(255, 102, 179), Color.FromArgb(255, 128, 128), Color.FromArgb(255, 0, 128), Color.FromArgb(192, 192, 192) };
        #endregion

        public Form1()
        {
            InitializeComponent();
            toolStripComboBox1.SelectedIndex = 0;
            grammar = new Grammar();
        }

        public TreeNode FormTree(ParseTreeNode node)
        {
            if (node.Children.Count == 0) return null;
            TreeNode root = new TreeNode(node.Name + ((node.Value == null || node.Value.Length == 0) ? "" : (" = " + node.Value)));
            root.NodeFont = new Font("Consolas", 8, FontStyle.Regular);
            #region Ensure all nodes having values
            bool allhavevalues = true;
            string value = "";
            foreach (ParseTreeNode p in node.Children)
            {
                allhavevalues = allhavevalues && (p.Value != null);
                if (allhavevalues)
                    value += p.Value + " ";
                else
                    break;
            }
            if (allhavevalues)
            {
                root.Text = node.Name + " = " + value;
                return root;
            }
            #endregion
            foreach (ParseTreeNode p in node.Children)
            {
                if (p.Name.Equals("__SYSTEM_GRAMMAR_REPGE"))
                {
                    foreach (ParseTreeNode p2 in p.Children)
                    {
                        TreeNode temp2 = FormTree(p2);
                        root.Nodes.Add(temp2);
                    }
                    continue;
                }
                else if (p.Name.Equals("__SYSTEM_GRAMMAR_LGE"))
                {
                    continue;
                }
                TreeNode temp = FormTree(p);
                if (temp != null)
                    root.Nodes.Add(temp);
            }
            if (root.Nodes.Count == 0) return null;
            return root;
        }
        public void ShowParseTree(ParseTreeNode node)
        {
            TreeNode root = FormTree(node);
            if (root != null)
            {
                treeView1.Nodes.Clear();
                treeView1.Nodes.Add(root);
            }
            textBox1.Text = node.ToCompiledParseTextRepresentation(parsedGrammar.GenerateTerminalMap(), int.Parse(toolStripTextBox1.Text));
        }
        public void ParseText()
        {
            string input = richTextBox2.Text;
            Infinity.Scripting.Utils.TestResult<bool> result = parsedGrammar.Parse(input);
            result.Data.Add("Remaining String", input);
            propertyGrid1.SelectedObject = result;
            if (result.Result)
                ShowParseTree((ParseTreeNode)result.Data["$PARSETREE.NODE$"]);
        }
        public void Highlight()
        {
            int current = richTextBox1.SelectionStart;
            int pivot = richTextBox1.Text.LastIndexOfAny(new char[] { ' ', '\n', '\t' }, current - 1);
            if (pivot < 0) pivot = 0;
            if (pivot < current)
            {
                int currents = richTextBox1.SelectionStart;
                int len = richTextBox1.SelectionLength;
                string text = richTextBox1.Text.Substring(pivot, current - pivot).Trim();
                bool changed = false;
                for (int i = 0; i < keyword_sets.Length; i++)
                {
                    if (keyword_sets[i].Contains(text))
                    {
                        richTextBox1.SelectionStart = pivot;
                        richTextBox1.SelectionLength = current - pivot;
                        richTextBox1.SelectionColor = keywordForeColors[i];
                        changed = true;
                        break;
                    }
                }
                if (!changed)
                {
                    richTextBox1.SelectionStart = pivot;
                    richTextBox1.SelectionLength = current - pivot;
                    richTextBox1.SelectionColor = editorForeColor;
                }
                richTextBox1.SelectionStart = currents;
                richTextBox1.SelectionLength = len;
            }
        }

        private void codeTextChanged(object sender, EventArgs e)
        {
            //Highlight();
            #region Clear Error Ranges
            if (_errorRanges != null)
            {
                int current = richTextBox1.SelectionStart;
                int len = richTextBox1.SelectionLength;
                foreach (TextRange _errorRange in _errorRanges)
                {
                    richTextBox1.SelectionStart = _errorRange.Start;
                    int last = _errorRange.Start + _errorRange.Length;
                    if (richTextBox1.Text.Length != 0)
                    {
                        if (last >= richTextBox1.Text.Length)
                            last = richTextBox1.Text.Length - 1;
                        int idx = richTextBox1.Text.IndexOf('\n', last) - _errorRange.Start;
                        richTextBox1.SelectionLength = (idx <= 0) ? richTextBox1.Text.Length : idx;
                        richTextBox1.SelectionBackColor = editorBackColor;
                        //richTextBox1.SelectionColor = Color.Black;
                    }
                }
                richTextBox1.SelectionStart = current;
                richTextBox1.SelectionLength = len;
            }
            #endregion
            #region Check if Valid And No Errors
            try
            {
                parsedGrammar = Infinity.Scripting.InfinityGrammarScript.LoadGrammar(richTextBox1.Text);
                toolStripStatusLabel1.Text = "No Errors";
                propertyGrid.SelectedObject = parsedGrammar;
                debugText.Text = "";
            }
            #endregion
            #region If Errors Found
            catch (Infinity.Scripting.InfinityGrammarScriptParseError ie)
            {
                toolStripStatusLabel1.Text = ie.Message + " line = " + ie.Line;
                _errorRanges = new List<TextRange>();
                if (ie.MultiError)
                {
                    string exception = ie.Message;
                    int current = richTextBox1.SelectionStart;
                    int len = richTextBox1.SelectionLength;
                    foreach (Infinity.Scripting.InfinityGrammarScriptParseError err in ie.Errors)
                    {
                        int start = richTextBox1.Text.IndexOf(err.Line.Trim());
                        int lent = err.Line.Trim().Length;
                        TextRange _errorRange = new TextRange(start, lent);
                        _errorRanges.Add(_errorRange);
                        richTextBox1.SelectionStart = start;
                        richTextBox1.SelectionLength = lent;
                        richTextBox1.SelectionBackColor = Color.FromArgb(255, 255, 160, 160);
                        //richTextBox1.SelectionColor = Color.White;
                        exception += "\n    " + err.Message + "\n        at " + err.Line;
                    }
                    richTextBox1.SelectionStart = current;
                    richTextBox1.SelectionLength = len;
                    debugText.Text = exception;
                    parsedGrammar = null;
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "Parse Error - " + ex.Message;
                string exception = ex.ToString() + "\nSource - " + ex.Source + "\n\t- " + ex.Message;
                debugText.Text = exception;
                parsedGrammar = null;
            }
            #endregion
            richTextBox1.Update();
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0) openFileDialog1.Filter = "Infinity Grammar Script Files (*.igrm)|*igrm";
            else if (tabControl1.SelectedIndex == 1) openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.ShowDialog();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                TextReader tr = new StreamReader(openFileDialog1.FileName);
                richTextBox1.Text = tr.ReadToEnd();
                richTextBox1.ScrollToCaret();
            } 
            else if(tabControl1.SelectedIndex == 1)
            {
                TextReader tr = new StreamReader(openFileDialog1.FileName);
                richTextBox2.Text = tr.ReadToEnd();
                richTextBox2.ScrollToCaret();
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (parsedGrammar != null)
            {
                ParseText();
            }
        }
        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                int lastSpace = -1, firstSpace = richTextBox1.Text.Length;
                string selected = null;
                if (richTextBox1.SelectionLength == 0)
                {
                    int caret = richTextBox1.SelectionStart;
                    if (caret >= richTextBox1.Text.Length)
                    {
                        propertyGrid.SelectedObject = parsedGrammar;
                        return;
                    }
                    lastSpace = richTextBox1.Text.LastIndexOfAny(new char[] { ' ', '\t' }, caret + 1);
                    firstSpace = richTextBox1.Text.IndexOfAny(new char[] { ' ', '\t' }, caret);
                    if (lastSpace < firstSpace)
                        selected = richTextBox1.Text.Substring(lastSpace + 1, firstSpace - lastSpace).Trim();
                }
                else
                {
                    int selStart = richTextBox1.SelectionStart;
                    if (selStart >= richTextBox1.Text.Length) return;
                    lastSpace = richTextBox1.Text.LastIndexOfAny(new char[] { ' ', '\t' }, selStart + 1);
                    int selEnd = richTextBox1.SelectionLength + selStart - 1;
                    firstSpace = richTextBox1.Text.IndexOfAny(new char[] { ' ', '\t' }, selEnd);
                    if (lastSpace < firstSpace)
                        selected = richTextBox1.SelectedText.Trim();
                }
                if (selected != null)
                {
                    if (parsedGrammar != null)
                    {
                        List<Infinity.Scripting.GrammarScriptItem> items = parsedGrammar.GetItems(selected);
                        object[] arr = new object[items.Count];
                        for (int i = 0; i < arr.Length; i++)
                        {
                            if (items[i].Type == Infinity.Scripting.GrammarScriptItemType.Symbol)
                                arr[i] = parsedGrammar.Symbols[items[i].Name];
                            else if (items[i].Type == Infinity.Scripting.GrammarScriptItemType.SymbolSet)
                                arr[i] = parsedGrammar.SymbolSets[items[i].Name];
                            else if (items[i].Type == Infinity.Scripting.GrammarScriptItemType.Terminal)
                                arr[i] = parsedGrammar.Terminals[items[i].Name];
                        }
                        propertyGrid.SelectedObjects = arr;
                    }
                }
                else
                {
                    propertyGrid.SelectedObject = parsedGrammar;
                }
            }
            catch (Exception) { propertyGrid.SelectedObject = parsedGrammar; }
        }
        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex == 0)
                toolStripButton1.Visible = false;
            else
                toolStripButton1.Visible = true;
        }
        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            if (toolStripComboBox1.SelectedIndex == 0 && parsedGrammar != null)
            {
                ParseText();
            }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }
        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            int factor = -1;
            bool what = int.TryParse(toolStripTextBox1.Text, out factor);
            if (!what)
                toolStripTextBox1.Text = factor + "";
            if (factor <= 0) toolStripTextBox1.Text = 1 + "";
            ParseText();
        }
        private void tabPage6_Click(object sender, EventArgs e)
        {
            Console.WriteLine(splitContainer3.Height);
        }


        //Grammar Designer
        Grammar grammar;

        private void addGrammarElementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewElement element = new NewElement();
            DialogResult result = element.ShowDialog();
            if (result == DialogResult.OK)
            {
                string name = element.ElementName;
                int type = element.ElementType;
                TreeNode root = treeView2.SelectedNode;
                if (root.Text.Equals("Grammar"))
                {
                    Infinity.Scripting.InfinityGrammarScriptExtension.ParseAndPutIntoGrammar(grammar, element.ElementStatement);
                    TreeNode newnode = new TreeNode(name, 1, 1);
                    newnode.ContextMenuStrip = root.ContextMenuStrip;
                    root.Nodes.Add(newnode);
                }
            }
        }

        private void treeView2_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode sel = treeView2.SelectedNode;
            if (!sel.Text.Equals("Grammar"))
            {
                try
                {
                    textBox1.Text = "GrammarElement " + textBox1.Text + " = ";
                    GrammarElement ge = grammar.Terminals[sel.Text];
                    propertyGrid2.SelectedObject = ge;
                }
                catch (Exception) { }
            }
        }
    }
}

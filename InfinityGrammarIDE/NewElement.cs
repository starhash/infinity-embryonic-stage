using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InfinityGrammarIDE
{
    public partial class NewElement : Form
    {
        int type;
        string name;
        string code;

        public int ElementType { get { return type; } }
        public string ElementName { get { return name; } }
        public string ElementStatement { get { return code; } }

        public NewElement()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Dispose();
            type = comboBox1.SelectedIndex;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Dispose();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string text = textBox2.Text;
            if (text.StartsWith("GrammarElement"))
            {
                text = text.Substring(15).Trim();
                if (text.Contains("="))
                {
                    text = text.Substring(text.IndexOf("=") + 1).Trim();
                    text = "GrammarElement " + textBox1.Text + " = " + text;
                }
                else
                {
                    textBox2.Text = "GrammarElement " + textBox1.Text + " = ";
                }
            }
            else
            {
                textBox2.Text = "GrammarElement " + textBox1.Text + " = ";
            }
            name = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            code = textBox2.Text;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            type = comboBox1.SelectedIndex;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BasicTriangle
{
    public partial class Form1 : Form
    {
        public delegate void ResizeClientType(int wdith, int height);
        public delegate void ExitProgramType();
        public delegate void SaveOutputType(string path);
        public delegate void CompileType(string shader);

        public ResizeClientType ResizeClient;
        public ExitProgramType ExitProgram;
        public SaveOutputType SaveOutput;
        public CompileType CompileCode;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string txt = this.comboBox1.Text;
            try
            {
                int size = int.Parse(txt);
                ResizeClient(size, size);
            }
            catch
            { }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ExitProgram();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            string filename = this.FileNameBox.Text;
            if (filename == null)
            {

            }
            if (this.FileNameBox.Text == "")
            {
                if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    filename = this.saveFileDialog1.FileName;
                    this.FileNameBox.Text = filename;
                }
                else
                    return;
            }
            SaveOutput(filename);
        }

        private void CompileButton_Click(object sender, EventArgs e)
        {
            CompileCode(this.shaderText.Text);
        }
    }
}

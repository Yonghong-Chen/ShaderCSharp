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
        public delegate void PauseShaderType(bool bPause);        

        public ResizeClientType ResizeClient;
        public ExitProgramType ExitProgram;
        public SaveOutputType SaveOutput;
        public CompileType CompileCode;
        public PauseShaderType PauseShader;
        public Form1()
        {
            for (int i = 0; i < frameDeltas.Length; i++)
                frameDeltas[i] = 0.0f;
               
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

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (this.buttonPause.Text == "Pause")
            {
                PauseShader(true);
                this.buttonPause.Text = "Resume"; 
            }
            else
            {
                this.buttonPause.Text = "Pause";
                PauseShader(false);
            }
        }

        private int frameRateIndex = 0;
        private float[] frameDeltas = new float[10];
        public void ReportFrameRate(float frameDelta)
        {
            frameDeltas[frameRateIndex++] = frameDelta;
            if (frameRateIndex >= frameDeltas.Length)
                frameRateIndex = 0;

            float count = 0.0f;
            float total = 0.0f;
            foreach(var delta in frameDeltas)
            {
                if (delta != 0.0f)
                {
                    total += delta;
                    count += 1.0f;
                }
            }
            if (count == 0.0f || total == 0.0f)
                lableFrameRate.Text = "";
            else
            {
                float average = total / count;
                float rate = 1.0f / average;
                int FrameRate = (int)rate;
                lableFrameRate.Text = string.Format("{0} fps");
            }
        }
    }
}

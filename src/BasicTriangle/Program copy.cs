using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BasicTriangle
{
    sealed class Program : GameWindow
    {
        Stopwatch stopwatch = new Stopwatch();
        List<iChannel> m_iChannels = new List<iChannel>();
        float m_iTime; //iTime
        float m_iTimeDelta; //time Delta
        int m_iFrame; //frame
        struct channelInfo
        {            
            public string path;
            public string type;
        };
        // A simple vertex shader possible. Just passes through the position vector.
        const string VertexShaderSource = @"
            #version 430           

            layout(location = 0) in vec4 vertex;
            layout(location = 1) in vec2 pos;
            out vec2 texPos;

            void main(void)
            {
                gl_Position = vertex;
                texPos = pos;
            }
        ";

        // A simple fragment shader. Just a constant red color.
        const string FragmentShaderSource = @"
            #version 330

            out vec4 outputColor;

            void main(void)
            {
                outputColor = vec4(1.0, 0.0, 0.0, 1.0);
            }
        ";

        // Points of a triangle in normalized device coordinates.
        float[] Points = new float[] {
            // X, Y, Z, W
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        };

        void FillTexturePosition()
        {
            float[] TexturePosition = new float[] {
            // X, Y, Z, W
            0.0f, 0.0f,
            Width, 0.0f,
            0.0f, Height,
            Width, Height,
            };
            for(int i = 0; i < 4; i ++)
            {
                Points[i * 6 + 4 + 0] = TexturePosition[i * 2 + 0];
                Points[i * 6 + 4 + 1] = TexturePosition[i * 2 + 1];
            }
        }

        int VertexShader;
        int FragmentShader;
        int ShaderProgram;
        int VertexBufferObject;
        int VertexArrayObject;

        void CompileShader(string vertex, string fragment)
        {
            GL.DeleteProgram(ShaderProgram);
            GL.DeleteShader(VertexShader);
            GL.DeleteShader(FragmentShader);

            string error = "";
            try
            {
                VertexShader = GL.CreateShader(ShaderType.VertexShader);
                GL.ShaderSource(VertexShader, vertex);
                GL.CompileShader(VertexShader);
                error = GL.GetShaderInfoLog(VertexShader);
                if (error != "")
                    throw new Exception(error);
 

                // Load the source of the fragment shader and compile it.
                FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
                GL.ShaderSource(FragmentShader, fragment);
                GL.CompileShader(FragmentShader);
                
                error = GL.GetShaderInfoLog(FragmentShader);
                if (error != "")
                    throw new Exception(error);

                // Create the shader program, attach the vertex and fragment shaders and link the program.
                ShaderProgram = GL.CreateProgram();
                GL.AttachShader(ShaderProgram, VertexShader);
                GL.AttachShader(ShaderProgram, FragmentShader);
                GL.LinkProgram(ShaderProgram);
            }
            catch
            {
                GL.DeleteProgram(ShaderProgram);
                GL.DeleteShader(VertexShader);
                GL.DeleteShader(FragmentShader);
                System.Windows.Forms.MessageBox.Show(error, "Failed to Compile Shader");
            }
            finally
            {
                m_iTime = 0.0f;
                m_iTimeDelta = 0.0f;
                m_iFrame = 0;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            CompileShader(VertexShaderSource, FragmentShaderSource);

            VertexBufferObject = GL.GenBuffer();
            // Bind the VBO and copy the vertex data into it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, BufferUsageHint.DynamicDraw);

            // Retrive the position location from the program.
            var positionLocation = GL.GetAttribLocation(ShaderProgram, "vertex");
            var textureLocation = GL.GetAttribLocation(ShaderProgram, "pos");
            textureLocation = 1;

            // Create the vertex array object (VAO) for the program.
            VertexArrayObject = GL.GenVertexArray();
            // Bind the VAO and setup the position attribute.
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, 6, 0);
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(textureLocation, 2, VertexAttribPointerType.Float, false, 6, 4);
            GL.EnableVertexAttribArray(textureLocation);

            // Set the clear color to blue
            GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);

            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteProgram(ShaderProgram);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            // Resize the viewport to match the window size.
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Clear the color buffer.
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

            FillTexturePosition();
            GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, BufferUsageHint.DynamicDraw);

            // Bind the VAO
            GL.BindVertexArray(VertexArrayObject);
            // Use/Bind the program
            GL.UseProgram(ShaderProgram);
            // This draws the triangle.
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

            // Swap the front/back buffers so what we just rendered to the back buffer is displayed in the window.
            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }
        void ResizeClient(int width, int height)
        {
            this.Size = new System.Drawing.Size(width, height);
        }
        void ExitProgram()
        {
            this.Exit();
        }
        void SaveOutput(string path)
        {
            int width = 1920;
            int height = 1080;
            using (Bitmap bitmap = new Bitmap(this.Width, this.Height))
            {
                System.Drawing.Imaging.BitmapData bits = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.ReadPixels(0, 0, this.Width, this.Height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bits.Scan0);
                bitmap.UnlockBits(bits);
                //bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                bitmap.Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        void CompileCode(string shaderCode)
        {
            //# iChannel0 "file://cubemaps/yokohama_{}.jpg" // Note the wildcard '{}'
            //# iChannel0::Type "CubeMap"
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string[] lines = shaderCode.Split(new char[] { '\r', '\n' });
            channelInfo[] infos = new channelInfo[10];
            foreach(string line in lines)
            {
                string txt = line.Trim();
                if (txt.StartsWith("#iChannel"))
                {
                    uint id = uint.Parse(txt.Substring(9, 1));
                    int startPos = txt.IndexOf('\"');
                    int endPos = txt.LastIndexOf('\"');
                    string txtData = txt.Substring(startPos + 1, endPos - startPos - 1);

                    if (txt.Substring(10).StartsWith("::Type"))
                    {
                        // get the text between the quotes;
                        infos[id].type = txtData;
                    }
                    else
                    {
                        infos[id].path = txtData;
                    }
                }
            }
            string standardVertex = @"
        #version 430
		layout(location = 0) in vec4 vertex;

        layout(location = 1) in vec2 pos;

        out vec2 texPos;

      

        void main () {

        gl_Position = vertex;

         texPos = pos;

        }";
            string standardHeader =
                @"
		#version 430\n
		layout(location = 0) out vec4 fragColor;

        in vec2 texPos;

        layout(location = 1) uniform float  iOpacity;

        layout(location = 2) uniform vec3  iResolution;

        layout(location = 3) uniform float  iTime;


        layout(location = 4) uniform float  iTimeDelta;

        layout(location = 5) uniform int  iFrame;

        layout(location = 6) uniform vec4  iMouse;


        {0}
        {1}

        void main() {

        mainImage(fragColor, texPos.xy);

        fragColor.a = fragColor.a * iOpacity;

        }
";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            m_iChannels.Clear();
            for (int i = 0; i < 10; i ++)
            {
                if (infos[i].path != "")
                {
                    if (infos[i].type == "") // no type, defaults to 2d texture
                    {
                        sb.AppendFormat("uniform sampler2D iChannel{0)", i);
                        m_iChannels.Add(new iChannel((uint) i, infos[i].path));
                    }
                }
            }
            sb.AppendLine();

            System.Text.StringBuilder sb2 = new System.Text.StringBuilder();
            sb2.AppendFormat(standardHeader, sb.ToString(), shaderCode);
            
            CompileShader(standardVertex, sb2.ToString());
        }
        void OnPrepareParameters()
        {
            stopwatch.Stop();
            m_iTimeDelta = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            m_iTime += m_iTimeDelta;
            GL.Uniform1(1, 1.0);
            GL.Uniform3(2, (float)Width, (float)Height, 1.0f); // last paramter is pixel ratio, eg 4:3, 16:9, we set it to 1
            GL.Uniform1(3, m_iTime); //iTime
            GL.Uniform1(4, m_iTimeDelta); //time Delta

            float frameDuration = 1.0f / 30.0f;
            m_iFrame = (int) (m_iTime / frameDuration);
            GL.Uniform1(5, m_iFrame); //frame

            //GL.Uniform4(6, m_iMouse.x, m_iMouse.y, 0.0f, 0.0f);
            GL.Uniform4(6, 0.0, 0.0, 0.0f, 0.0f);

            foreach(iChannel channel in this.m_iChannels)
            {
                string name = string.Format("iChannel{0}", channel.id);

                TextureUnit[] units = new TextureUnit[] {
                    TextureUnit.Texture0,
                    TextureUnit.Texture1,
                    TextureUnit.Texture2,
                    TextureUnit.Texture3,
                    TextureUnit.Texture4,
                    TextureUnit.Texture5,
                    TextureUnit.Texture6,
                    TextureUnit.Texture7,
                    TextureUnit.Texture8,
                    TextureUnit.Texture9,
                };
                GL.ActiveTexture(units[channel.id]);
                GL.BindTexture(TextureTarget.Texture2D, channel.textureID);
                GL.Uniform1(GL.GetUniformLocation(ShaderProgram, name), channel.id);
            }
        }
        [STAThread]
        static void Main()
        {
            var program = new Program();

            Form1 form1 = new Form1();
            form1.Show();
            form1.ResizeClient = program.ResizeClient;
            form1.ExitProgram = program.ExitProgram;
            form1.SaveOutput = program.SaveOutput;
            form1.CompileCode = program.CompileCode;
            program.Run();
        }
    }
}

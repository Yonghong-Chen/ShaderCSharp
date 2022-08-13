using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace BasicTriangle
{
    public class iChannel
    {
        public enum ChannelType
        {
            Texture2D,
            Texture3D,
            CubeMap,
        };
        public uint id;
        public int textureID;
        public string path;
        public ChannelType type;
        public int width;
        public int height;

        public iChannel(uint id, string path, ChannelType type)
        {
            this.id = id;
            this.path = path;
            this.type = type;
            this.textureID = 0;
            width = height = 0;
        }
        public iChannel(uint id, string path)
        {
            this.id = id;
            this.path = path;
            this.type = ChannelType.Texture2D;
            this.textureID = 0;
            width = height = 0;
        }
        public void Unload()
        {
            if (this.textureID != 0)
            {
                GL.DeleteTexture(this.textureID);
            }
        }

        public void Load2DTexture(string path)
        {
            Uri u = new Uri(path);
            System.IO.FileStream fs = System.IO.File.OpenRead(u.LocalPath);
            using (Image<Rgba32> image = Image.Load<Rgba32>(fs))
            {
                //Use the CopyPixelDataTo function from ImageSharp to copy all of the bytes from the image into an array that we can give to OpenGL.
                var pixels = new byte[4 * image.Width * image.Height];
                image.CopyPixelDataTo(pixels);
                this.textureID = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, this.textureID);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                var error = GL.GetError();
                width = image.Width;
                height = image.Height;
            }
        }
        public void Load()
        {
            try
            {
                if (this.type == ChannelType.Texture2D)
                {
                    Load2DTexture(this.path);
                }
            }
            catch (Exception e)
            {
                if (this.textureID != 0)
                {
                    GL.DeleteTexture(this.textureID);
                }
                System.Windows.Forms.MessageBox.Show(e.ToString(), "Cannot Open Texture File");
            }        
        }
        public int GetGLId()
        {
            return this.textureID;
            
        }
    }
}

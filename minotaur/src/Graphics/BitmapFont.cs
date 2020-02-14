using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
    public class BitmapFont : GraphicsResource
    {
        private BitmapCharacterSet _characterSet;
        private Program _shader;
        private Texture2D _texture;
        
        public float Ascent { get; internal set; }
        public float Descent { get; internal set; }

        public BitmapCharacterSet CharacterSet { get { return _characterSet; } }

        public Texture2D Texture
        {
          get { return _texture; }
          set { _texture = value; }
        }

        public BitmapFont(Program shader)
        {
            _characterSet = new BitmapCharacterSet();
            _shader = shader;
        }

        public Program Shader { get { return _shader; } }

        public void Bind()
        {
          _shader.Bind();
          _shader.BindUniforms(new Dictionary<string, IUniformValue>()
          {
            {"Texture", UniformValue.Create(Sampler.Create(_texture))}
          });
        }

        // load and parse a BMFont file
        public void ParseFNTFile(string fntFile)
        {
            using (StreamReader stream = new StreamReader(fntFile))
            {
                string line;
                char[] seperators = new char[] { ' ', '=' };
                while ((line = stream.ReadLine()) != null)
                {
                    string[] tokens = line.Split(seperators, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens[0] == "info")
                    {
                        for (int i = 1; i < tokens.Length; i += 2)
                        {
                            if (tokens[i] == "size")
                            {
                                _characterSet.RenderedSize = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "padding")
                            {
                                int[] padding = tokens[i + 1].Split(new char[] { ',' }).Select(p => int.Parse(p)).ToArray();
                                _characterSet.PaddingUp = padding[0];
                                _characterSet.PaddingRight = padding[1];
                                _characterSet.PaddingDown = padding[2];
                                _characterSet.PaddingLeft = padding[3];
                            }
                        }
                    }
                    else if (tokens[0] == "common")
                    {
                        // BitmapCharacterSet fields
                        for (int i = 1; i < tokens.Length; i += 2)
                        {
                            if (tokens[i] == "lineHeight")
                            {
                                _characterSet.LineHeight = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "base")
                            {
                                _characterSet.Base = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "scaleW")
                            {
                                _characterSet.Width = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "scaleH")
                            {
                                _characterSet.Height = int.Parse(tokens[i + 1]);
                            }
                        }
                    }
                    else if (tokens[0] == "char")
                    {
                        // New Bitmap Character
                        BitmapCharacter character = new BitmapCharacter();
                        for (int i = 1; i < tokens.Length; i += 2)
                        {
                            if (tokens[i] == "id")
                            {
                                character.ID = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "x")
                            {
                                character.X = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "y")
                            {
                                character.Y = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "width")
                            {
                                character.Width = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "height")
                            {
                                character.Height = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "xoffset")
                            {
                                character.XOffset = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "yoffset")
                            {
                                character.YOffset = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "xadvance")
                            {
                                character.XAdvance = int.Parse(tokens[i + 1]);
                            }
                        }
                        _characterSet.Characters.Add(character);
                    }
                    else if (tokens[0] == "kerning")
                    {
                        int index = 0;
                        int second = 0;
                        int amount = 0;
                        for (int i = 1; i < tokens.Length; i += 2)
                        {
                            if (tokens[i] == "first")
                            {
                                index = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "second")
                            {
                                second = int.Parse(tokens[i + 1]);
                            }
                            else if (tokens[i] == "amount")
                            {
                                amount = int.Parse(tokens[i + 1]);
                            }
                        }
                        BitmapCharacter character = _characterSet.GetCharacterByID(index);
                        if (character != null)
                            character.Kerning[second] = amount;
                    }
                }
            }
        }

        public Vector2 MeasureString(string s)
        {
            int height = _characterSet.LineHeight;
            int width = 0;
            int lineWidth = 0;
            for (int i = 0; i < s.Length; i++)
            {
                BitmapCharacter bc = _characterSet.GetCharacterByID(s[i]);
                if (s[i] == '\n')
                {
                    height += _characterSet.LineHeight;
                    lineWidth += _characterSet.GetCharacterByID(s[i - 1]).Width;
                    if (lineWidth > width)
                        width = lineWidth;
                    lineWidth = 0;
                }
                //else if (i == s.Length - 1)  // last character
                //{
                //  lineWidth += bc.XOffset + _characterSet.PaddingLeft + bc.Width - (_characterSet.PaddingLeft + _characterSet.PaddingRight);
                //}
                else
                {
                    lineWidth += (int)bc.XAdvance;
                }
            }
            lineWidth += _characterSet.GetCharacterByID(s[s.Length - 1]).Width;
            if (lineWidth > width)
                width = lineWidth;
            return new Vector2(width, height);
        }

        public void DrawString(string s, int x, int y, int size)
        {
            float curX = x;
            float curY = y;

            // TODO: replace with VAO creation
            GL.Begin(BeginMode.Quads);
            float scaleSize = size / (float)_characterSet.RenderedSize;

            foreach (char c in s)
            {
                BitmapCharacter bc = _characterSet.GetCharacterByID((int)c);

                // upper left
                GL.TexCoord2((float)bc.X / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height);
                GL.Vertex2(curX + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize);

                // upper right
                GL.TexCoord2((float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height);
                GL.Vertex2(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize);

                // lower right
                GL.TexCoord2((float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height);
                GL.Vertex2(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize);

                // lower left
                GL.TexCoord2((float)bc.X / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height);
                GL.Vertex2(curX + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize);

                curX += bc.XAdvance * scaleSize;
            }

            GL.End();
        }

        public void DrawInto(SpriteBatch spriteBatch, string s, int x, int y, int size, Color4 color, float depth = 0f)
        {
          float curX = x;
          float curY = y;

          float scaleSize = size != 0 ? size / (float)_characterSet.RenderedSize : 1f;

          foreach (char c in s)
          {
            if (c == '\n')
            {
              curY += _characterSet.LineHeight;
              curX = x;
              continue;
            }
            BitmapCharacter bc = _characterSet.GetCharacterByID((int)c);
            spriteBatch.Draw(new Sprite(_texture, bc.X, bc.Y, bc.Width, bc.Height),
              (int)(curX + bc.XOffset * scaleSize), (int)(curY + bc.YOffset * scaleSize), (int)(bc.Width * scaleSize), (int)(bc.Height * scaleSize), color);
            curX += bc.XAdvance * scaleSize;
          }
        }

        public VertexPositionTexture[] Build(string s, int x, int y, int size, out ushort[] indices)
        {
          return Build(s, x, y, size, 0, out indices);
        }

        public VertexPositionTexture[] Build(string s, int x, int y, int size, float depth, out ushort[] indices)
        {
          List<ushort> indexList = new List<ushort>();
          List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

          float curX = x;
          float curY = y;

          float scaleSize = size != 0 ? size / (float)_characterSet.RenderedSize : 1f;

          foreach (char c in s)
          {
            if (c == '\n')
            {
              curY += _characterSet.LineHeight;
              curX = x;
              continue;
            }
            BitmapCharacter bc = _characterSet.GetCharacterByID((int)c);

            // upper left
            vertices.Add(new VertexPositionTexture(curX + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize, depth,
              (float)bc.X / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height));
            // upper right
            vertices.Add(new VertexPositionTexture(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize, depth,
              (float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height));
            // lower left
            vertices.Add(new VertexPositionTexture(curX + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize, depth,
              (float)bc.X / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height));
            // lower right
            vertices.Add(new VertexPositionTexture(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize, depth,
              (float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height));
            
            indexList.Add(0);
            indexList.Add(2);
            indexList.Add(1);
            indexList.Add(1);
            indexList.Add(2);
            indexList.Add(3);

            curX += bc.XAdvance * scaleSize;
          }

          indices = indexList.ToArray();
          return vertices.ToArray();
        }

        public VertexPositionColorTexture[] Build(string s, Matrix4 world, int size, Color4 color, out ushort[] indices)
        {
          List<ushort> indexList = new List<ushort>();
          List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
          ushort index = 0;

          float curX = 0;
          float curY = 0;

          float scaleSize = size != 0 ? size / (float)_characterSet.RenderedSize : 1f;
          Vector4 vcolor = new Vector4(color.R, color.G, color.B, color.A);

          foreach (char c in s)
          {
            if (c == '\n')
            {
              curY += _characterSet.LineHeight;
              curX = 0;
              continue;
            }
            BitmapCharacter bc = _characterSet.GetCharacterByID((int)c);

            // upper left
            vertices.Add(new VertexPositionColorTexture(Vector3.Transform(new Vector3(curX + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize, 0), world),
              vcolor,
              new Vector2((float)bc.X / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height)));
            // upper right
            vertices.Add(new VertexPositionColorTexture(Vector3.Transform(new Vector3(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.YOffset * scaleSize, 0), world),
              vcolor,
              new Vector2((float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)bc.Y / (float)_characterSet.Height)));
            // lower left
            vertices.Add(new VertexPositionColorTexture(Vector3.Transform(new Vector3(curX + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize, 0), world),
              vcolor,
              new Vector2((float)bc.X / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height)));
            // lower right
            vertices.Add(new VertexPositionColorTexture(Vector3.Transform(new Vector3(curX + bc.Width * scaleSize + bc.XOffset * scaleSize, curY + bc.Height * scaleSize + bc.YOffset * scaleSize, 0), world),
              vcolor,
              new Vector2((float)(bc.X + bc.Width) / (float)_characterSet.Width, (float)(bc.Y + bc.Height) / (float)_characterSet.Height)));

            indexList.Add((ushort)(index + 0));
            indexList.Add((ushort)(index + 2));
            indexList.Add((ushort)(index + 1));
            indexList.Add((ushort)(index + 1));
            indexList.Add((ushort)(index + 2));
            indexList.Add((ushort)(index + 3));

            index += 4;
            curX += bc.XAdvance * scaleSize;
          }

          indices = indexList.ToArray();
          return vertices.ToArray();
        }
    }
}

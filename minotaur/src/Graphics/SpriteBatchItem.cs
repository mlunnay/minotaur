using OpenTK;
using OpenTK.Graphics;

namespace Minotaur.Graphics
{
  internal class SpriteBatchItem
  {
    public ISprite Sprite;
    public Matrix4 World = Matrix4.Identity;

    //public Texture2D Texture;
    public Texture2D Texture { get { return Sprite.Texture; } }
    public float Depth;
    public int X;
    public int Y;
    public int Width;
    public int Height;
    public SpriteEffect SpriteEffect;

    public Color4 Color;

    public SpriteBatchItem()
    {
      Color = new Color4(1f, 1f, 1f, 1f);
      Depth = 0f;
      SpriteEffect = Graphics.SpriteEffect.None;
    }
  }
}

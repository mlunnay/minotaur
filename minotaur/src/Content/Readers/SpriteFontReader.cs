using System;
using System.Linq;
using Minotaur.Core;
using Minotaur.Graphics;

namespace Minotaur.Content
{
  public class SpriteFontReader : ContentTypeReader<BitmapFont>
  {
    private Program _shader;

    public SpriteFontReader(Program shader)
      : base(new Guid("1f6057f0-d13f-42ae-9e6b-4011fad823fd")) 
    {
      _shader = shader;
    }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<Texture2D>(new Texture2DReader());
    }

    public override object Read(ContentReader reader)
    {
      Texture2D texture = reader.ReadObjectRaw<Texture2D>();
      BitmapFont font = new BitmapFont(_shader);
      font.Texture = texture;
      font.CharacterSet.LineHeight = reader.ReadInt32();
      font.CharacterSet.Base = reader.ReadInt32();
      font.CharacterSet.RenderedSize = reader.ReadInt32();
      font.CharacterSet.PaddingUp = reader.ReadInt32();
      font.CharacterSet.PaddingRight = reader.ReadInt32();
      font.CharacterSet.PaddingDown = reader.ReadInt32();
      font.CharacterSet.PaddingLeft = reader.ReadInt32();
      font.CharacterSet.Width = texture.Width;
      font.CharacterSet.Height = texture.Height;
      int characterCount = reader.ReadInt32();
      for (int i = 0; i < characterCount; i++)
      {
        BitmapCharacter character = new BitmapCharacter();
        character.ID = reader.ReadInt32();
        character.X = reader.ReadInt32();
        character.Y = reader.ReadInt32();
        character.Width = reader.ReadInt32();
        character.Height = reader.ReadInt32();
        character.XOffset = reader.ReadInt32();
        character.YOffset = reader.ReadInt32();
        character.XAdvance = reader.ReadInt32();
        int kerningCount = reader.ReadInt32();
        for (int j = 0; j < kerningCount; j++)
        {
          int key = reader.ReadInt32();
          int value = reader.ReadInt32();
          character.Kerning[key] = value;
        }
        font.CharacterSet.Characters.Add(character);
      }

      font.Ascent = font.CharacterSet.Characters.Where(c => c.ID > 33 && c.ID < 127).Max(c => c.YOffset);
      font.Descent = font.CharacterSet.Characters.Where(c => c.ID > 33 && c.ID < 127).Max(c => c.Height - c.YOffset);

      return font;
    }
  }
}

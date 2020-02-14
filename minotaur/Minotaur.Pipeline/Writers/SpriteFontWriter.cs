using System;
using System.Collections.Generic;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class SpriteFontWriter : ContentTypeWriter<SpriteFontContent>
  {
    public SpriteFontWriter()
      : base(new Guid("1f6057f0-d13f-42ae-9e6b-4011fad823fd")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Texture2DContent>(new Texture2DWriter());
    }

    public override void Write(ContentWriter writer, SpriteFontContent value)
    {
      writer.WriteRawObject(value.Texture);
      writer.Write(value.CharacterSet.LineHeight);
      writer.Write(value.CharacterSet.Base);
      writer.Write(value.CharacterSet.RenderSize);
      writer.Write(value.CharacterSet.PaddingUp);
      writer.Write(value.CharacterSet.PaddingRight);
      writer.Write(value.CharacterSet.PaddingDowm);
      writer.Write(value.CharacterSet.PaddingLeft);
      writer.Write(value.CharacterSet.Characters.Count);
      foreach (SpriteFontContent.Character character in value.CharacterSet.Characters)
      {
        writer.Write(character.ID);
        writer.Write(character.X);
        writer.Write(character.Y);
        writer.Write(character.Width);
        writer.Write(character.Height);
        writer.Write(character.XOffset);
        writer.Write(character.YOffset);
        writer.Write(character.XAdvance);
        writer.Write(character.Kerning.Count);
        foreach (KeyValuePair<int, int> pair in character.Kerning)
        {
          writer.Write(pair.Key);
          writer.Write(pair.Value);
        }
      }
    }
  }
}

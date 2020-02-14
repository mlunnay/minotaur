using System.IO;
using System.Linq;
using Minotaur.Pipeline.Graphics;
using System.Collections.Generic;

namespace Minotaur.Pipeline.Importers
{
  [ContentImporter(".fnt",
    DefaultProcessor = "SpriteFontProcessor",
    DisplayName = "Sprite Font Importer")]
  public class SpriteFontImporter : ContentImporter<SpriteFontContent>
  {
    public override SpriteFontContent Import(FileStream stream, ContentManager manager)
    {
      SpriteFontContent font = new SpriteFontContent();
      using (StreamReader reader = new StreamReader(stream))
      {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
          string[] tokens = line.Split(' ').Select(s => s.Trim()).ToArray();
          if (tokens[0] == "info")
          {
            foreach (string token in tokens.Skip(1))
            {
              string[] data = token.Split(new[] { '=' }, 2);
              if (data[0] == "size")
                font.CharacterSet.RenderSize = int.Parse(data[1]);
              else if (data[0] == "padding")
              {
                int[] splits = data[1].Split(',').Select(s => int.Parse(s)).ToArray();
                font.CharacterSet.PaddingUp = splits[0];
                font.CharacterSet.PaddingRight = splits[1];
                font.CharacterSet.PaddingDowm = splits[2];
                font.CharacterSet.PaddingLeft = splits[3];
              }
            }
          }
          else if (tokens[0] == "common")
          {
            foreach (string token in tokens.Skip(1))
            {
              string[] data = token.Split(new[] { '=' }, 2);
              if (data[0] == "lineHeight")
                font.CharacterSet.LineHeight = int.Parse(data[1]);
              else if (data[0] == "base")
                font.CharacterSet.Base = int.Parse(data[1]);
            }
          }
          else if (tokens[0] == "page")
          {
            if (tokens[1] != "id=0")
              continue; // importer only handles fonts with single textures.
            string texturePath = Path.Combine(Path.GetDirectoryName(stream.Name), tokens[2].Split(new char[] { '=' }, 2)[1].Trim('"'));
            // TODO: make texture output a greyscale image
            font.Texture = (Texture2DContent)manager.LoadContentItem(texturePath,
            opaqueData: new Dictionary<string, object>()
              {
                {"TextureType", TextureType.Texture2D},
                {"PremultiplyAlpha", false},
                {"GenerateMipmaps", false},
                {"OutputType", ImageType.R8}
              });
          }
          else if (tokens[0] == "char")
          {
            SpriteFontContent.Character character = new SpriteFontContent.Character();
            foreach (string token in tokens.Skip(1))
            {
              string[] data = token.Split(new[] { '=' }, 2);
              if (data[0] == "id")
                character.ID = int.Parse(data[1]);
              else if (data[0] == "x")
                character.X = int.Parse(data[1]);
              else if (data[0] == "y")
                character.Y = int.Parse(data[1]);
              else if (data[0] == "width")
                character.Width = int.Parse(data[1]);
              else if (data[0] == "height")
                character.Height = int.Parse(data[1]);
              else if (data[0] == "xoffset")
                character.XOffset = int.Parse(data[1]);
              else if (data[0] == "yoffset")
                character.YOffset = int.Parse(data[1]);
              else if (data[0] == "xadvance")
                character.XAdvance = int.Parse(data[1]);
            }
            font.CharacterSet.Characters.Add(character);
          }
          else if (tokens[0] == "kerning")
          {
            int index = -1;
            int second = -1;
            int amount = int.MinValue;
            foreach (string token in tokens.Skip(1))
            {
              string[] data = token.Split(new[] { '=' }, 2);
              if (data[0] == "first")
                index = int.Parse(data[1]);
              else if (data[0] == "second")
                second = int.Parse(data[1]);
              else if (data[0] == "amount")
                amount = int.Parse(data[1]);
            }
            if(index == -1 && second == -1 && amount == int.MinValue)
              throw new ContentException(string.Format("FontImporter read incorrect kerning line in file {0}", stream.Name));

            SpriteFontContent.Character character = font.CharacterSet.Characters.FirstOrDefault(c => c.ID == index);
            if (character != null)
              character.Kerning[second] = amount;
          }
        }
      }

      return font;
    }
  }
}

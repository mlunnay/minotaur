using System;
using Assimp;
using Minotaur.Pipeline.Graphics;
using System.IO;

namespace Minotaur.Pipeline.Importers
{
  [ContentImporter(".dae",
    ".blend",
    ".3ds",
    ".ase",
    ".obj",
    ".lwo",
    ".lws",
    ".X",
    DisplayName="Model Importer",
    DefaultProcessor="ModelProcessor")]
  public class ModelImporter : ContentImporter<SceneContent>
  {

    public override SceneContent Import(FileStream stream, ContentManager manager)
    {
      AssimpImporter importer = new AssimpImporter();
      Scene scene = importer.ImportFileFromStream(stream, PostProcessPreset.TargetRealTimeMaximumQuality, Path.GetExtension(stream.Name));

      return new SceneContent(scene) { Name = stream.Name };
    }
  }
}

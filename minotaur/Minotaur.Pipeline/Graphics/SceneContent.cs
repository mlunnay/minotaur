using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;

namespace Minotaur.Pipeline.Graphics
{
  public class SceneContent : ContentItem
  {
    public Scene Scene { get; private set; }

    public SceneContent(Scene scene)
    {
      Scene = scene;
    }
  }
}

using System.Collections.Generic;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.MaterialFactories
{
  public interface IMaterialFactory
  {
    MaterialContent CreateMaterial(ContentProcessorContext context, Dictionary<string, object> parameters); 
  }
}

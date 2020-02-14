using System;
using System.Collections.Generic;
using System.Linq;

namespace Minotaur.Graphics
{
  public class RenderPipeline
  {
    private static Dictionary<string, IPipelineCommand> _commandMap = new Dictionary<string, IPipelineCommand>();
    private Dictionary<string, FrameBuffer> _renderTargets = new Dictionary<string, FrameBuffer>();
    private List<RenderPipelineStage> _stages = new List<RenderPipelineStage>();
    private Dictionary<string, Texture> _samplerBindings = new Dictionary<string, Texture>(); // samplername, texture to bind

    public ICamera Camera { get; set; }

    public Dictionary<string, Texture> SamplerBindings { get { return _samplerBindings; } }

    public FrameBuffer CurrentRenderTarget { get; set; }

    public RenderPipeline()
    {
    }

    static RenderPipeline()
    {
      // add the default inbuilt commands
      Type type = typeof(IPipelineCommand);
      IEnumerable<Type> types = typeof(RenderPipeline).Assembly.GetTypes().Where(p => type.IsAssignableFrom(p) && p != type);
      foreach (Type t in types)
      {
        object[] attributes = t.GetCustomAttributes(typeof(PipelineCommandAttribute), false);
        string name;
        if (attributes.Length == 0)
        {
          // if no PipelineCommandAttribute set the command name to the class name with Command removed from the end.
          name = t.Name;
          if (name.EndsWith("Command"))
            name = name.Substring(0, name.Length - 7);
        }
        else
        {
          name = ((PipelineCommandAttribute)attributes[0]).Name;
        }
        if (_commandMap.ContainsKey(name))
          throw new ApplicationException(string.Format("Attempted to add command with duplicate name: {0}", name));
        IPipelineCommand command = Activator.CreateInstance(t) as IPipelineCommand;
        _commandMap[name] = command;
      }
    }

    public static void AddCommand(string name, IPipelineCommand command)
    {
      _commandMap[name] = command;
    }

    /// <summary>
    /// Add a pipeline command getting its command name from a PipelineCommandAttribute, or its name with Command removed from the end.
    /// </summary>
    /// <param name="command"></param>
    public static void AddCommand(IPipelineCommand command)
    {
      object[] attributes = command.GetType().GetCustomAttributes(typeof(PipelineCommandAttribute), false);
      string name;
      if (attributes.Length == 0)
      {
        // if no PipelineCommandAttribute set the command name to the class name with Command removed from the end.
        name = command.GetType().Name;
        if (name.EndsWith("Command"))
          name = name.Substring(0, name.Length - 7);
      }
      else
      {
        name = ((PipelineCommandAttribute)attributes[0]).Name;
      }
      if (_commandMap.ContainsKey(name))
        throw new ApplicationException(string.Format("Attempted to add command with duplicate name: {0}", name));
      _commandMap[name] = command;
    }

    public static IPipelineCommand GetCommand(string name)
    {
      IPipelineCommand command;
      if (!_commandMap.TryGetValue(name, out command))
        throw new KeyNotFoundException(string.Format("Command {0} is not registered.", name));
      return command;
    }

    public FrameBuffer GetRenderTarget(string name)
    {
      FrameBuffer target;
      if (!_renderTargets.TryGetValue(name, out target))
        return null;
      return target;
    }

    public void AddSamplerBinding(string name, Texture texture)
    {
      _samplerBindings[name] = texture;
    }

    public void ClearSamplerBindings()
    {
      _samplerBindings.Clear();
    }
  }
}

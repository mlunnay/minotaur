using System;
using System.Collections.Generic;
using OpenTK.Graphics;

namespace Minotaur.Core
{
  /// <summary>
  /// This is a utility class to do disposal operations in a controlled manner on the thread with a GL Context
  /// </summary>
  public static class DisposalManager
  {
    private static List<Action> _actions = new List<Action>();

    public static void Add(Action action)
    {
      lock (_actions)
      {
        _actions.Add(action);
      }
    }

    public static void Process()
    {
      if (GraphicsContext.CurrentContext == null)
        throw new InvalidOperationException("DisplosalManager.Process called in a thread without an OpenGL context.");

      lock (_actions)
      {
        foreach (Action action in _actions)
        {
          action();
        }
        _actions.Clear();
      }
    }

    public static bool HasContext()
    {
      return GraphicsContext.CurrentContext != null;
    }
  }
}

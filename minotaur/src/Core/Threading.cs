using System;
using System.Threading;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

namespace Minotaur.Core
{
  internal class Threading
  {
    private static int _mainThreadId;
    public static IGraphicsContext BackgroundContext;
    public static IWindowInfo WindowInfo;

    static Threading()
    {
      _mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    /// <summary>
    /// Checks if the current thread is the UI thread
    /// </summary>
    /// <returns>True if the code is running on the UI thread.</returns>
    public static bool IsOnUIThread()
    {
      return _mainThreadId == Thread.CurrentThread.ManagedThreadId;
    }

    public static void EnsureUIThread()
    {
      if (_mainThreadId != Thread.CurrentThread.ManagedThreadId)
        throw new InvalidOperationException(String.Format("Operation not called on UI thread. UI thread ID = {0}. This thread ID = {1}.", _mainThreadId, Thread.CurrentThread.ManagedThreadId));
    }

    /// <summary>
    /// Runs the given action on the UI thread and blocks the current thread while the action is running.
    /// If the current thread is the UI thread the action will run immediately.
    /// </summary>
    /// <param name="action">The action to run on the UI thread.</param>
    public static void BlockOnUIThread(Action action)
    {
      if (action == null)
        throw new ArgumentNullException("action");

      // if this is the main thread execute immediatly
      if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
      {
        action();
        return;
      }

      lock (BackgroundContext)
      {
        BackgroundContext.MakeCurrent(WindowInfo);
        action();
        GL.Flush();
        Graphics.Utilities.CheckGLError();
        BackgroundContext.MakeCurrent(null);
      }
    }
  }
}


namespace Minotaur.Components
{
  public interface IGameComponent
  {
    //bool SetupDone { get; }

    /// <summary>
    /// Called when a Game Component is successfully added to an Entity.
    /// This is used to register callbacks etc.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Called before the first update is called.
    /// This is used to set up global communication etc.
    /// </summary>
    void Setup();
  }
}

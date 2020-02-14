
namespace Minotaur.Graphics
{
  public class FPS
  {
    private int _updateCount;
    private int _currentFrame;
    private int _counter;
    private double _elapsedTimeSinceLastUpdate;
    private double _fps;
    private double _overallFps;

    public int CurrentFrame { get { return _currentFrame; } }

    public double OverallFramesPerSecond { get { return _overallFps; } }

    public double FramesPerSecond { get { return _fps; } }

    public FPS()
    {
      Reset();
    }

    public void Reset()
    {
      _updateCount = 0;
      _currentFrame = 0;
      _counter = 0;
      _elapsedTimeSinceLastUpdate = 0;
      _fps = 0f;
      _overallFps = 0f;
    }

    public void Update(double elapsedTime)
    {
      _counter++;
      _currentFrame++;

      _elapsedTimeSinceLastUpdate += elapsedTime;
      if (_elapsedTimeSinceLastUpdate >= 1.0f)
      {
        _fps = 1000 * _counter / _elapsedTimeSinceLastUpdate;
        _counter = 0;
        _elapsedTimeSinceLastUpdate -= 1.0f;

        _overallFps = (_overallFps * _updateCount + _fps) / (_updateCount + 1);
        _updateCount++;
      }
    }
  }
}

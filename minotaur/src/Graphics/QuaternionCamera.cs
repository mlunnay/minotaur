using System;
using OpenTK;

namespace Minotaur.Graphics
{
  /// <summary>
  /// Camera that can perform Quaternion rotation and look in all six directions
  /// </summary>
  public class QuaternionCamera : ICamera
  {
    #region Declarations

    // camera orientation quaternion style
    private Quaternion _orientation;

    // camera position - default (0,0,0)
    private Vector3 _position;

    private float _fieldOfView;
    private float _nearPlaneDistance;
    private float _farPlaneDistance;
    private float _aspectRatio;

    private Matrix4 _viewMatrix;
    private Matrix4 _projectionMatrix;
    private bool _needUpdateViewMatrix;
    private bool _needUpdateProjectionMatrix;

    #endregion

    #region Properties

    public Matrix4 View
    {
      get
      {
        UpdateViewMatrix();
        return _viewMatrix;
      }
    }

    public Matrix4 Projection
    {
      get
      {
        UpdateProjectionMatrix();
        return _projectionMatrix;
      }
    }

    public Matrix4 Matrix { get { return View * Projection; } }

    public Vector3 Position
    {
      get { return _position; }
      set
      {
        _position = value;
        _needUpdateViewMatrix = true;
      }
    }

    public Quaternion Orientation
    {
      get { return _orientation; }
      set
      {
        _orientation = value;
        _needUpdateViewMatrix = true;
      }
    }

    public float NearPlane
    {
      get { return _nearPlaneDistance; }
      set
      {
        _nearPlaneDistance = value;
        _needUpdateProjectionMatrix = true;
      }
    }

    public float FarPlane
    {
      get { return _farPlaneDistance; }
      set
      {
        _farPlaneDistance = value;
        _needUpdateProjectionMatrix = true;
      }
    }

    public float AspectRation
    {
      get { return _aspectRatio; }
      set
      {
        _aspectRatio = value;
        _needUpdateProjectionMatrix = true;
      }
    }

    public float FieldOfView
    {
      get { return _fieldOfView; }
      set
      {
        _fieldOfView = value;
        _needUpdateProjectionMatrix = true;
      }
    }

    #endregion

    #region Constructor

    public QuaternionCamera()
    {
      _orientation = Quaternion.Identity;
      _position = Vector3.Zero;

      _nearPlaneDistance = 100.0f;
      _farPlaneDistance = 100000.0f;
      _aspectRatio = 4.0f / 3.0f;
      _fieldOfView = (float)Math.PI / 4.0f;

      _needUpdateViewMatrix = true;
      _needUpdateProjectionMatrix = true;
      _viewMatrix = _projectionMatrix = Matrix4.Identity;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Moves the camera's position by the vector offset provided along world axes.
    /// </summary>
    /// <param name="translate"></param>
    public void Move(Vector3 translate)
    {
      _position += translate;
      _needUpdateViewMatrix = true;
    }

    /// <summary>
    /// Moves the camera's position by the vector offset provided along the camera's axes.
    /// </summary>
    /// <param name="translate"></param>
    public void MoveRelative(Vector3 translate)
    {
      Vector3 trans = Vector3.Transform(translate, _orientation);
      _position += trans;
      _needUpdateViewMatrix = true;
    }

    /// <summary>
    /// Rolls the camera anti-clockwise around its local z axis
    /// </summary>
    /// <param name="angle"></param>
    public void Roll(float angle)
    {
      Vector3 zAxis = Vector3.Transform(Vector3.UnitZ, _orientation);
      Rotate(zAxis, angle);
    }

    /// <summary>
    /// Rotates the camera anti-clockwise around its local y axis.
    /// </summary>
    /// <param name="angle"></param>
    public void Yaw(float angle)
    {
      Vector3 yAxis = Vector3.Transform(Vector3.UnitY, _orientation);
      Rotate(yAxis, angle);
    }

    /// <summary>
    /// Rotates the camera anti-clockwise around its local x axis.
    /// </summary>
    /// <param name="angle"></param>
    public void Pitch(float angle)
    {
      Vector3 xAxis = Vector3.Transform(Vector3.UnitX, _orientation);
      Rotate(xAxis, angle);
    }

    /// <summary>
    /// Rotate the camera around an arbitrary axis.
    /// </summary>
    /// <param name="axis"></param>
    /// <param name="angle"></param>
    public void Rotate(Vector3 axis, float angle)
    {
      Quaternion q = Quaternion.FromAxisAngle(axis, angle);
      Rotate(q);
    }

    /// <summary>
    /// Rotate the camera by a Quaternion
    /// </summary>
    /// <param name="q"></param>
    public void Rotate(Quaternion q)
    {
      Quaternion qnorm = Quaternion.Normalize(q);
      _orientation *= qnorm;
      _needUpdateViewMatrix = true;
    }

    #endregion

    #region Private Methods

    private void UpdateProjectionMatrix()
    {
      if (_needUpdateProjectionMatrix)
      {
        _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspectRatio, _nearPlaneDistance, _farPlaneDistance);
        _needUpdateProjectionMatrix = false;
      }
    }

    private void UpdateViewMatrix()
    {
      if (_needUpdateViewMatrix)
      {
        _viewMatrix = Matrix4.CreateTranslation(-_position) * Matrix4.Rotate(Quaternion.Invert(_orientation));
        _needUpdateViewMatrix = false;
      }
    }

    #endregion
  }
}

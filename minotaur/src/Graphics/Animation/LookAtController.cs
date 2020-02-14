using System;
using Minotaur.Core;
using Minotaur.Helpers;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Controls the target bone to make it look at the specified target.
  /// </summary>
  public class LookAtController : IBoneAnimationController, IUpdateable
  {
    private Vector3 _forward;
    private Vector3 _up;
    private Vector3 _currentLookAt;
    private Quaternion _desiredRotation;
    private float _desiredBlendWeight;
    private bool _hasTargetLastFrame;

    /// <summary>
    /// Gets or sets the index of the controlled bone.
    /// </summary>
    public int Bone { get; set; }

    /// <summary>
    /// Gets or sets the target to look at.
    /// </summary>
    public Vector3? Target { get; set; }

    /// <summary>
    /// Gets or sets the up axis.
    /// </summary>
    public Vector3 Up {
      get { return _up; }
      set { _up = Vector3.Normalize(value); }
    }

    /// <summary>
    /// Gets or sets the Forward axis
    /// </summary>
    public Vector3 Forward
    {
      get { return _forward; }
      set { _forward = _currentLookAt = Vector3.Normalize(value); }
    }

    /// <summary>
    /// Gets or sets the base world transform of the parent BoneAnimation.
    /// </summary>
    public Matrix4 Transform { get; set; }

    /// <summary>
    /// The maximum horizontal rotation allowed.
    /// </summary>
    public float HorizontalRotationMax { get; set; }

    /// <summary>
    /// The minimum horizontal rotation allowed.
    /// </summary>
    public float HorizontalRotationMin { get; set; }

    /// <summary>
    /// The maximum vertical rotation allowed.
    /// </summary>
    public float VerticalRotationMax { get; set; }

    /// <summary>
    /// The minimum vertical rotation allowed.
    /// </summary>
    public float VerticalRotationMin { get; set; }

    /// <summary>
    /// The maximum rotation speed
    /// </summary>
    public float RotationSpeed { get; set; }

    /// <summary>
    /// The skeleton that this controller is affecting.
    /// </summary>
    public Skeleton Skeleton { get; private set; }

    public event EventHandler TargetSpotted;
    public event EventHandler TargetLost;

    /// <summary>
    /// LookAtController constructor
    /// </summary>
    /// <param name="skeleton">The skeleton to modify.</param>
    /// <param name="transform">The parent world transform.</param>
    /// <param name="bone">The id of the bone to control.</param>
    public LookAtController(Skeleton skeleton, Matrix4 transform, int bone)
    {
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");

      Transform = transform;
      Skeleton = skeleton;
      RotationSpeed = (float)Math.PI;
      HorizontalRotationMax = VerticalRotationMax = (float)Math.PI;
      HorizontalRotationMin = VerticalRotationMin = -(float)Math.PI;
      Up = Vector3.UnitZ;
      Forward = -Vector3.UnitY;
      Bone = bone;

      _currentLookAt = Forward;
      _desiredRotation = Quaternion.Identity;
    }

    public bool TryGetBoneTransforms(int bone, out Vector3 translation, out Quaternion rotation, out Vector3 scale, out float blendWeight)
    {
      blendWeight = _desiredBlendWeight;
      translation = Vector3.Zero;
      scale = Vector3.One;
      rotation = _desiredRotation;

      return bone == Bone;
    }

    public bool TryGetGoneTransforms(int bone, out Matrix4 transform, out float blendWeight)
    {
      transform = Matrix4.Rotate(_desiredRotation);
      blendWeight = _desiredBlendWeight;

      return bone == Bone;
    }

    public void Update(GameClock clock)
    {
      bool hasTarget = Target.HasValue;

      if (hasTarget)
      {
        Matrix4 parentWorldTransform = Transform;
        Bone parentBone;
        if ((parentBone = Skeleton.Bones[Bone].Parent) != null)
          parentWorldTransform = Skeleton.GetAbsoluteBoneTransform(parentBone.Index) * Transform;

        Vector3 parentLocal = Vector3.Transform(Target.Value, Matrix4.Invert(parentWorldTransform));

        // compute the rotation to the target position.
        Vector3 desiredLookAt = Vector3.Normalize(parentLocal - new Vector3(Skeleton.Bones[Bone].Transform.Row3));

        if (desiredLookAt == Vector3.Zero)
        {
          hasTarget = false;
          desiredLookAt = Forward;
        }

        float horizontalAngle = (float)Math.Acos(Vector3.Dot(desiredLookAt, Forward));
        float verticalAngle = (float)Math.Acos(Vector3.Dot(desiredLookAt, Up));

        // clamp the rotation
        bool clamped = false;
        if (horizontalAngle > HorizontalRotationMax)
        {
          horizontalAngle = HorizontalRotationMax;
          clamped = true;
        }
        else if (horizontalAngle < HorizontalRotationMin)
        {
          horizontalAngle = HorizontalRotationMin;
          clamped = true;
        }
        if (verticalAngle > VerticalRotationMax)
        {
          verticalAngle = VerticalRotationMax;
          clamped = true;
        }
        else if (verticalAngle < VerticalRotationMin)
        {
          verticalAngle = VerticalRotationMin;
          clamped = true;
        }

        if (clamped)
        {
          desiredLookAt = Vector3.Transform(Vector3.One, Quaternion.FromAxisAngle(_up, verticalAngle) * Quaternion.FromAxisAngle(_forward, horizontalAngle));
        }

        float maxRotation = (float)(RotationSpeed * clock.ElapsedSeconds);
        float currentRotation = (float)Math.Acos(Vector3.Dot(desiredLookAt, _currentLookAt));
        if (currentRotation > maxRotation)
        {
          desiredLookAt = Vector3.Normalize(Vector3.Lerp(_currentLookAt, desiredLookAt, maxRotation / currentRotation));
        }
        _currentLookAt = desiredLookAt;

        Quaternion parentRotation = QuaternionHelper.FromMatrix(parentWorldTransform);
        // not sure of this final calculation
        _desiredRotation = parentRotation * QuaternionHelper.RotationBetweenVectors(desiredLookAt, Up);
      }
    }
  }
}

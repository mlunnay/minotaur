using System;
using System.Collections.Generic;
using OpenTK;

namespace Minotaur.Graphics.Primitives
{
  public class Sphere : PrimitiveBase
  {
    private Vector3[] _positions;
    private ushort[] _indices;

    protected override IEnumerable<Vector3> Positions
    {
      get { return _positions; }
    }

    protected override IEnumerable<ushort> Indices
    {
      get { return _indices; }
    }

    public Sphere(float radius, int tessellation = 16, bool solid = false)
      : base(solid ? OpenTK.Graphics.OpenGL.BeginMode.Triangles : OpenTK.Graphics.OpenGL.BeginMode.Lines)
    {
      if (tessellation < 3) throw new ArgumentOutOfRangeException("tessellation", "Must be >= 3");

      int verticalSegments = tessellation;
      //int horizontalSegments = tessellation * 2;
      int horizontalSegments = tessellation;

      _positions = new Vector3[(horizontalSegments - 1) * verticalSegments + 2];
      if(solid)
        _indices = new ushort[((horizontalSegments * 2) + ((verticalSegments - 2) * (horizontalSegments * 2))) * 3];
      else
      _indices = new ushort[((verticalSegments * horizontalSegments) + ((verticalSegments - 1) * horizontalSegments)) * 2];

      int vertexCount = 0;
      // Create rings of vertices at progressively higher latitudes.
      for (int i = 0; i <= verticalSegments; i++)
      {

        var latitude = (float)((i * Math.PI / verticalSegments) - Math.PI / 2.0);
        var dy = (float)Math.Sin(latitude);
        var dxz = (float)Math.Cos(latitude);

        if (dy == 1 || dy == -1)
        {
          _positions[vertexCount++] = new Vector3(0f, dy, 0f) * radius;
        }
        else
        {
          // Create a single ring of vertices at this latitude.
          for (int j = 0; j < horizontalSegments; j++)
          {

            var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
            var dx = (float)Math.Sin(longitude);
            var dz = (float)Math.Cos(longitude);

            dx *= dxz;
            dz *= dxz;

            var normal = new Vector3(dx, dy, dz);

            _positions[vertexCount++] = new Vector3(dx, dy, dz) * radius;
          }
        }
      }

      // Fill the index buffer with triangles joining each pair of latitude rings.
      int stride = horizontalSegments;

      int indexCount = 0;
      if (solid)
      {
        // do bottom first.
        for (int j = 0; j < horizontalSegments; j++)
        {
          int nextJ = (j + 1) % stride;
          _indices[indexCount++] = (ushort)(0);
          _indices[indexCount++] = (ushort)(1 + j);
          _indices[indexCount++] = (ushort)(1 + nextJ);
        }
        for (int i = 0; i < verticalSegments - 2; i++)
        {
          for (int j = 0; j < horizontalSegments; j++)
          {
            // do line up to next segment and to the next horizontal vertex.
            int nextI = i + 1;
            int nextJ = (j + 1) % stride;

            _indices[indexCount++] = (ushort)(1 + i * stride + j);
            _indices[indexCount++] = (ushort)(1 + nextI * stride + j);
            _indices[indexCount++] = (ushort)(1 + i * stride + nextJ);

            _indices[indexCount++] = (ushort)(1 + i * stride + nextJ);
            _indices[indexCount++] = (ushort)(1 + nextI * stride + j);
            _indices[indexCount++] = (ushort)(1 + nextI * stride + nextJ);
          }
        }
        // top line
        for (int j = 0; j < horizontalSegments; j++)
        {
          int nextJ = (j + 1) % stride;

          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 1) * stride);
          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 2) * stride + j);
          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 2) * stride + nextJ);
        }
      }
      else
      {
        // do bottom first, no horizontal lines.
        for (int i = 0; i < horizontalSegments; i++)
        {
          _indices[indexCount++] = (ushort)(0);
          _indices[indexCount++] = (ushort)(1 + i);
        }
        for (int i = 0; i < verticalSegments - 2; i++)
        {
          for (int j = 0; j < horizontalSegments; j++)
          {
            // do line up to next segment and to the next horizontal vertex.
            int nextI = i + 1;
            int nextJ = (j + 1) % stride;

            _indices[indexCount++] = (ushort)(1 + i * stride + j);
            _indices[indexCount++] = (ushort)(1 + nextI * stride + j);

            _indices[indexCount++] = (ushort)(1 + i * stride + j);
            _indices[indexCount++] = (ushort)(1 + i * stride + nextJ);
          }
        }
        // top line
        for (int j = 0; j < horizontalSegments; j++)
        {
          int nextJ = (j + 1) % stride;

          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 2) * stride + j);
          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 2) * stride + nextJ);

          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 2) * stride + j);
          _indices[indexCount++] = (ushort)(1 + (verticalSegments - 1) * stride);
        }
      }
    }
  }
}

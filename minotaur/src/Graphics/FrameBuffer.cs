using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Core;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class FrameBuffer : GraphicsResource
  {
    private struct TextureAttachment
    {
      public Texture Texture;
      public FramebufferAttachment Attachment;
    }

    private static int _maxAttacments = -1;

    private int _id;
    private Texture2D[] _textures;
    private List<TextureAttachment> _textureAttachments = new List<TextureAttachment>();
    private Texture2D _depthTexture;
    private DrawBuffersEnum[] _drawBuffers;
    private int _numDrawBuffers;

    public int ID { get { return _id; } }

    public Texture2D[] Textures { get { return _textures; } }
    public Texture2D DepthTexture { get { return _depthTexture; } }

    public int Width { get; private set; }
    public int Height { get; private set; }

    public float TexelSizeX { get; private set; }
    public float TexelSizeY { get; private set; }

    public FramebufferErrorCode Status { get; private set; }

    public DrawBuffersEnum[] DrawBuffers { get { return _drawBuffers; } }

    /// <summary>
    /// This constructor gets the default framebuffer (the one used to draw to screen).
    /// </summary>
    /// <param name="device"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="target"></param>
    public FrameBuffer(GraphicsDevice device, int width, int height, FramebufferTarget target = FramebufferTarget.Framebuffer)
    {
      _id = 0;
      Bind(target);
      Init(device);
      Width = width;
      Height = height;
      TexelSizeX = 1f / (float)Width;
      TexelSizeY = 1f / (float)Height;
    }

    public FrameBuffer(GraphicsDevice device, FramebufferTarget target = FramebufferTarget.Framebuffer)
    {
      GL.GenFramebuffers(1, out _id);
      Bind(target);
      Init(device);
    }

    public Texture GetAttachment(FramebufferAttachment attachment)
    {
      return _textureAttachments.FirstOrDefault(ta => ta.Attachment == attachment).Texture;
    }

    public void Attach(Texture2D texture, FramebufferAttachment attachment, int level = 0)
    {
      GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, texture.Target, texture.ID, level);
      Utilities.CheckGLError();

      AddTextureAttachment(new TextureAttachment() { Texture = texture, Attachment = attachment });
    }

    public void Attach(TextureCube texture, TextureTarget face, FramebufferAttachment attachment, int level = 0)
    {
      if (face != TextureTarget.TextureCubeMapPositiveX ||
        face != TextureTarget.TextureCubeMapNegativeX ||
        face != TextureTarget.TextureCubeMapPositiveY ||
        face != TextureTarget.TextureCubeMapNegativeY ||
        face != TextureTarget.TextureCubeMapPositiveZ ||
        face != TextureTarget.TextureCubeMapNegativeZ)
        throw new ArgumentException("face must be one of the TextureCubeMap.. values of TextureTarget.");

      GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, face, texture.ID, level);
      Utilities.CheckGLError();

      AddTextureAttachment(new TextureAttachment() { Texture = texture, Attachment = attachment });
    }

    public void Bind(FramebufferTarget target = FramebufferTarget.Framebuffer)
    {
      GL.BindFramebuffer(target, _id);
    }

    public void BindDraw()
    {
      Bind(FramebufferTarget.DrawFramebuffer);
    }

    public void BindRead()
    {
      Bind(FramebufferTarget.ReadFramebuffer);
    }

    public void Unbind()
    {
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public static FrameBuffer Create(GraphicsDevice device, int numColorBuffers, PixelInternalFormat textureFormat = PixelInternalFormat.Rgba8, float scale = 1.0f, bool depthTexture = true)
    {
      int width = (int)(device.Width * scale);
      int height = (int)(device.Height * scale);
      return Create(device, numColorBuffers, textureFormat, width, height, depthTexture);
    }

    public static FrameBuffer Create(GraphicsDevice device, int numColorBuffers, PixelInternalFormat textureFormat = PixelInternalFormat.Rgba8, int width = 256, int height = 256, bool depthTexture = true)
    {
      FrameBuffer fb = new FrameBuffer(device);
      for (int i = 0; i < numColorBuffers; i++)
			{
			  fb.Attach(Texture2D.CreateEmpty(width, height, textureFormat), FramebufferAttachment.ColorAttachment0 + i);
			}
      if (depthTexture)
        fb.Attach(Texture2D.CreateEmpty(width, height, PixelInternalFormat.DepthComponent16), FramebufferAttachment.DepthAttachment);
      return fb;
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        // no managed object to dispose so skipping if disposing
        DisposalManager.Add(() => GL.DeleteFramebuffers(1, ref _id));
        _id = 0;
      }
      IsDisposed = true;
    }

    private void AddTextureAttachment(TextureAttachment texa)
    {
      int index = _textureAttachments.FindIndex(a => a.Attachment == texa.Attachment);
      if (index == -1)
      {
        if (_textureAttachments.Count == _maxAttacments)
          throw new IndexOutOfRangeException("Maximum number of textures have already been attached to the FrameBuffer.");
        _textureAttachments.Add(texa);
        SetDrawBuffers();
      }
      else
      {
        _textureAttachments[index] =  texa;
      }
      SetSize();
      SetStatus();
    }

    private void SetDrawBuffers()
    {
      _numDrawBuffers = 0;
      foreach (TextureAttachment item in _textureAttachments)
      {
        switch (item.Attachment)
        {
          case FramebufferAttachment.DepthAttachment:
          case FramebufferAttachment.DepthStencilAttachment:
          case FramebufferAttachment.StencilAttachment:
            break; // not a color attachment so do nothing
          default:
            _drawBuffers[_numDrawBuffers++] = (DrawBuffersEnum)item.Attachment;
            break;
        }
      }
      GL.DrawBuffers(_numDrawBuffers, _drawBuffers.Take(_numDrawBuffers).ToArray());
      Utilities.CheckGLError();
    }

    private void SetStatus()
    {
      Status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
    }

    private void SetSize()
    {
      foreach (TextureAttachment item in _textureAttachments)
      {
        // set the width / height to the min of all attached texture sizes.
        if (Width == 0 || Width > item.Texture.Width)
          Width = item.Texture.Width;
        if (Height == 0 || Height > item.Texture.Height)
          Height = item.Texture.Height;
      }
      TexelSizeX = 1f / (float)Width;
      TexelSizeY = 1f / (float)Height;
    }

    private void Init(GraphicsDevice device)
    {
      if (_maxAttacments == -1)
      {
        _maxAttacments = device.MaxColorAttachments + 2; // add two to allow for depth and stencil attachments  
      }
      _drawBuffers = new DrawBuffersEnum[device.MaxDrawBuffers];
      TexelSizeX = 0f;
      TexelSizeY = 0f;
    }
  }
}

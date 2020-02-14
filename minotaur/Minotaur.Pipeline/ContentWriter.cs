using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Ionic.Zlib;

namespace Minotaur.Pipeline
{
  public class ContentWriter : BinaryWriter
  {
    #region Declarations

    private bool _disposed;
    private const byte _version = 1;
    private Stream _outStream;
    private static Encoding _encoding = Encoding.UTF8;
    private bool _compressContent;
    private Stream _headerStream;
    private Stream _bodyStream;
    private string _identifierString;
    private Dictionary<ContentTypeWriter, int> _typeMap = new Dictionary<ContentTypeWriter, int>();
    private Dictionary<object, int> _sharedResourceMap = new Dictionary<object,int>();
    private List<object> _sharedResources = new List<object>();
    private List<ContentTypeWriter> _contentWriterList = new List<ContentTypeWriter>();
    private ContentTypeWriterManager _manager;
    private ContentManager _contentManager;

    #endregion

    #region Properties

    public ContentTypeWriterManager TypeWriterManager { get { return _manager; } }

    public ContentManager ContentManager { get { return _contentManager; } }

    public bool IsDisposed
    {
      get { return _disposed; }
      protected set { _disposed = value; }
    }

    #endregion

    #region Constructor

    internal ContentWriter(ContentTypeWriterManager manager, ContentManager contentManager, Stream output, bool compressOutput, string identifierString)
      : base(output, _encoding)
    {
      _manager = manager;
      _contentManager = contentManager;
      _headerStream = new MemoryStream();
      _bodyStream = new MemoryStream();
      _identifierString = identifierString;
      _compressContent = compressOutput;
      _outStream = OutStream;
      OutStream = _bodyStream;
    }

    #endregion

    #region Public Methods

    public override void Flush()
    {
      WriteSharedResources();
      WriteHeader();

      using (MemoryStream contentStream = new MemoryStream())
      {
        OutStream = contentStream;
        WriteTypeWriters();
        _bodyStream.Position = 0;
        _bodyStream.CopyTo(contentStream);

        // assemble the seperate streams into the final output stream
        OutStream = _outStream;
        _headerStream.Position = 0;
        _headerStream.CopyTo(_outStream);
         
        if (_compressContent)
          WriteCompressedStream(contentStream);
        else
          WriteUncompressedStream(contentStream);
      }

      base.Flush();
    }

    public override void Write(string value)
    {
      Write((uint)value.Length);
      int byteCount = _encoding.GetByteCount(value);
      byte[] buffer = new byte[256];
      int maxChars = 256 / _encoding.GetMaxByteCount(1);
      if (byteCount <= 256)
      {
        _encoding.GetBytes(value, 0, value.Length, buffer, 0);
        Write(buffer, 0, byteCount);
        return;
      }
      int index = 0;
      int size = 0;
      for (int i = value.Length; i > 0; i-=size)
      {
        size = i > maxChars ? maxChars : i;
        int bytes = _encoding.GetBytes(value, index, size, buffer, 0);
        Write(buffer, 0, bytes);
        index += size;
      }
    }

    public void Write(Guid value)
    {
      Write(value.ToByteArray());
    }

    public void Write(Enum e)
    {
      Write(Convert.ToInt32(e));
    }

    public void WriteObject<T>(T value)
    {
      if (value == null)
      {
        Write(0);
        return;
      }

      ContentTypeWriter writer = _manager.GetTypeWriter(value.GetType());
      int index;
      if (!_typeMap.TryGetValue(writer, out index))
      {
        index = _contentWriterList.Count;
        _contentWriterList.Add(writer);
        _typeMap.Add(writer, index);
      }

      if (!writer.IsPrimitiveType)
        Write(index + 1);

      writer.Write(this, value);
    }

    public void WriteObject<T>(T value, ContentTypeWriter writer)
    {
      if (value == null)
      {
        Write(0);
        return;
      }

      int index;
      if (!_typeMap.TryGetValue(writer, out index))
      {
        index = _contentWriterList.Count;
        _contentWriterList.Add(writer);
        _typeMap.Add(writer, index);
      }

      if (!writer.IsPrimitiveType)
        Write(index + 1);

      writer.Write(this, value);
    }

    public void WriteRawObject<T>(T value)
    {
      ContentTypeWriter writer = _manager.GetTypeWriter(typeof(T));
      int index;
      if (!_typeMap.TryGetValue(writer, out index))
      {
        index = _contentWriterList.Count;
        _contentWriterList.Add(writer);
        _typeMap.Add(writer, index);
      }

      writer.Write(this, value);
    }

    public void WriteRawObject<T>(T value, ContentTypeWriter writer)
    {
      writer.Write(this, value);
    }

    public void WriteSharedResource(object value)
    {
      if (value == null)
        Write(0);
      else
      {
        int index;
        if (!_sharedResourceMap.TryGetValue(value, out index))
        {
          index = _sharedResources.Count;
          _sharedResources.Add(value);
          _sharedResourceMap.Add(value, index);
        }
        Write(index + 1);
      }
    }

    #endregion

    #region Private Methods

    private void WriteTypeWriters()
    {
      Write(_contentWriterList.Count);
      foreach (ContentTypeWriter writer in _contentWriterList)
      {
        Write(writer.ID);
      }
      Write(_sharedResources.Count);
    }

    private void WriteHeader()
    {
      OutStream = _headerStream;
      foreach (char c in _identifierString.ToCharArray())
      {
        Write(c);
      }
      Write(_version);
      byte flags = 0;
      if (_compressContent)
        flags |= 0x1;
      Write(flags);
    }

    private void WriteSharedResources()
    {
      foreach (object resource in _sharedResources)
      {
        WriteObject<object>(resource);
      }
    }

    private void WriteCompressedStream(Stream stream)
    {
      using (ZlibStream compressedStream = new ZlibStream(stream, CompressionMode.Compress, true))
      {
        long fileSize = _headerStream.Length + compressedStream.Length + sizeof(long);
        Write(fileSize);
        compressedStream.Position = 0;
        compressedStream.CopyTo(_outStream);
      }
    }

    private void WriteUncompressedStream(Stream stream)
    {
      long fileSize = _headerStream.Length + stream.Length + sizeof(long);
      Write(fileSize);
      stream.Position = 0;
      stream.CopyTo(_outStream);
    }

    protected override void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {
          OutStream = _outStream;
          if (_headerStream != null)
          {
            _headerStream.Dispose();
            _headerStream = null;
          }
          if (_bodyStream != null)
          {
            _bodyStream.Dispose();
            _bodyStream = null;
          }
        }
        _disposed = true;
      }

      base.Dispose(disposing);
    }

    #endregion
  }
}

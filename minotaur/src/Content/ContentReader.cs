using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Minotaur.Content
{
  public class ContentReader : IDisposable
  {
    #region Declarations

    private bool _disposed;
    private static Byte _version = 1;
    private BinaryReader _input;
    private static Encoding _encoding = Encoding.UTF8;
    private List<ContentTypeReader> _contentReaderList;
    private List<Action<object>>[] _sharedResourceCallbacks;
    private ContentTypeReaderManager _manager;
    private ContentManager _contentManager;

    #endregion

    #region Properties

    public ContentManager ContentManager { get { return _contentManager; } }

    public ContentTypeReaderManager TypeReaderManager { get { return _manager; } }

    #endregion

    #region Constructor and Destructor

    private ContentReader(ContentTypeReaderManager manager, ContentManager contentManager, Stream input)
    {
      _manager = manager;
      _contentManager = contentManager;
      _input = new BinaryReader(input);
      _contentReaderList = new List<ContentTypeReader>();
    }

    ~ContentReader()
    {
      Dispose(false);
    }

    #endregion

    #region Public Methods

    public byte ReadByte()
    {
      return _input.ReadByte();
    }

    public sbyte ReadSByte()
    {
      return _input.ReadSByte();
    }

    public bool ReadBoolean()
    {
      return _input.ReadBoolean();
    }

    public char ReadChar()
    {
      char[] data = new char[1];
      _input.Read(data, 0, 1);
      return data[0];
    }

    public short ReadInt16()
    {
      return _input.ReadInt16();
    }

    public int ReadInt32()
    {
      return _input.ReadInt32();
    }

    public long ReadInt64()
    {
      return _input.ReadInt64();
    }

    public ushort ReadUInt16()
    {
      return _input.ReadUInt16();
    }

    public uint ReadUInt32()
    {
      return _input.ReadUInt32();
    }

    public ulong ReadUInt64()
    {
      return _input.ReadUInt64();
    }

    public float ReadSingle()
    {
      return _input.ReadSingle();
    }

    public double ReadDouble()
    {
      return _input.ReadDouble();
    }

    public decimal ReadDecimal()
    {
      return _input.ReadDecimal();
    }

    public byte[] ReadBytes(int count)
    {
      System.Diagnostics.Debug.WriteLine("ReadBytes at {0}", _input.BaseStream.Position);
      return _input.ReadBytes(count);
    }

    public string ReadString()
    {
      int length = _input.ReadInt32();
      byte[] bytes = _input.ReadBytes(length);
      return _encoding.GetString(bytes);
    }

    public Guid ReadGuid()
    {
      return new Guid(_input.ReadBytes(16));
    }

    public T ReadEnum<T>()
    {
      return (T)Enum.ToObject(typeof(T), _input.ReadInt32());
    }

    public T ReadAsset<T>()
    {
      T result;
      try
      {
        int sharedResourceCount = ReadHeader();
        result = ReadObject<T>();
        ReadSharedResources(sharedResourceCount);
      }
      catch (IOException inner)
      {
        throw new ContentLoadException(string.Format("Error Reading asset of Type {0}", typeof(T)), inner);
      }
      return result;
    }

    public T ReadObject<T>()
    {
      int index = _input.ReadInt32();
      if (index == 0)
      {
        return default(T);
      }
      index--;
      if (index > _contentReaderList.Count)
        throw new ContentLoadException(string.Format("ContentTypeReader index {0} is greater than number of ContentTypeReaders {1}", index, _contentReaderList.Count));

      ContentTypeReader reader = _contentReaderList[index];
      return InvokeReader<T>(reader);
    }

    public T ReadObject<T>(ContentTypeReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      if (reader.IsPrimitiveType)
        return InvokeReader<T>(reader);
      else
        return ReadObject<T>();
    }

    public T ReadObjectRaw<T>()
    {
      ContentTypeReader reader = _manager.GetTypeReader(typeof(T));

      return InvokeReader<T>(reader);
    }

    public T ReadObjectRaw<T>(ContentTypeReader reader)
    {
      return InvokeReader<T>(reader);
    }

    public void ReadSharedResource<T>(Action<T> callback)
    {
      if (callback == null)
        throw new ArgumentNullException("callback");
      int index = _input.ReadInt32();
      if (index == 0)
        return;

      index--;
      if (index >= _sharedResourceCallbacks.Length)
        throw new ContentLoadException(string.Format("Shared resource index {0} is greater than number of shared resources {1}", index, _sharedResourceCallbacks.Length));
      _sharedResourceCallbacks[index].Add(obj =>
      {
        if (!(obj is T))
          throw new ContentLoadException(string.Format("Shared resource at {0} is not of Type {1}", index, typeof(T)));
        callback((T)obj);
      });
    }

    public T ReadExternalReference<T>()
    {
      string path = ReadString();
      if (string.IsNullOrEmpty(path))
        return default(T);
      return (T)_contentManager.Get<T>(path).Content;
    }

    #endregion

    #region Static Methods

    public static ContentReader Create(ContentTypeReaderManager manager, ContentManager contentManager, Stream input)
    {
      return Create(manager, contentManager, input, "MEB");
    }

    public static ContentReader Create(ContentTypeReaderManager manager, ContentManager contentManager, Stream input, string identifier)
    {
      input = PrepareStream(input, identifier);
      return new ContentReader(manager, contentManager, input);
    }

    public static Stream PrepareStream(Stream input)
    {
      return PrepareStream(input, "MEB");
    }

    public static Stream PrepareStream(Stream input, string identifier)
    {
      Stream result;
      try
      {
        BinaryReader binaryReader = new BinaryReader(input);
        foreach (char c in identifier)
        {
          if (binaryReader.ReadByte() != (Byte)c)
            throw new ContentLoadException(string.Format("File identifier bytes do not match: {0}", identifier));
        }
        if (binaryReader.ReadByte() != _version)
          throw new ContentLoadException(string.Format("Format version does not match: {0}", _version));
        byte flags = binaryReader.ReadByte();

        long fileSize = binaryReader.ReadInt64();
        long headerLength = (long)(identifier.Length + 10);
        if (input.CanSeek && (fileSize - headerLength) != input.Length - input.Position)
          throw new ContentLoadException("File size does not match header size");
        if ((flags & 0x1) != 0)
        {
          result = new DeflateStream(input, CompressionMode.Decompress);
        }
        else
        {
          result = input;
        }
      }
      catch (IOException innerException)
      {
        throw new ContentLoadException("Error reading content file", innerException);
      }

      return result;
    }

    #endregion

    #region Private Methods

    private T InvokeReader<T>(ContentTypeReader reader)
    {
      return (T)reader.Read(this);
    }

    private int ReadHeader()
    {
      int typeCount = _input.ReadInt32();
      for (int i = 0; i < typeCount; i++)
      {
        Guid readerID = ReadGuid();
        ContentTypeReader reader = _manager.GetTypeReaderByID(readerID);
        _contentReaderList.Add(reader);
      }
      int resourceCount = _input.ReadInt32();

      _sharedResourceCallbacks = new List<Action<object>>[resourceCount];
      for (int i = 0; i < resourceCount; i++)
      {
        _sharedResourceCallbacks[i] = new List<Action<object>>();
      }

      return resourceCount;
    }

    internal void ReadSharedResources(int count)
    {
      for (int i = 0; i < count; i++)
      {
        object obj = ReadObject<object>();
        foreach (Action<object> callback in _sharedResourceCallbacks[i])
        {
          callback(obj);
        }
      }
    }

    private void Pump(Stream input, Stream output)
    {
      int n;
      byte[] bytes = new byte[4096]; // 4KiB at a time

      while ((n = input.Read(bytes, 0, bytes.Length)) != 0)
      {
        output.Write(bytes, 0, n);
      }
    }

    //private void Pump(BinaryReader input, Stream output)
    //{
    //  int n;
    //  byte[] bytes = new byte[4096]; // 4KiB at a time

    //  while ((n = input.Read(bytes, 0, bytes.Length)) != 0)
    //  {
    //    output.Write(bytes, 0, n);
    //  }
    //}

    #endregion

    #region IDisposable Methods

    public bool IsDisposed
    {
      get { return _disposed; }
      protected set { _disposed = value; }
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposed)
      {
        if (disposing)
        {

        }
      }

      _disposed = true;
    }

    #endregion
  }
}

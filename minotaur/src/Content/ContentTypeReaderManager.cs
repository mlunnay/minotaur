using System;
using System.Collections.Generic;

namespace Minotaur.Content
{
  public class ContentTypeReaderManager
  {
    private Dictionary<Type, ContentTypeReader> _contentTypeMap = new Dictionary<Type, ContentTypeReader>();

    public ContentTypeReaderManager()
    {
      AddDefaultReaders();
    }

    public void RegisterTypeReader<T>(ContentTypeReader reader)
    {
      RegisterTypeReader(typeof(T), reader);
    }

    public void RegisterTypeReader(Type type, ContentTypeReader reader)
    {
      if (!_contentTypeMap.ContainsKey(type))
      {
        reader.Initialize(this);
        _contentTypeMap.Add(type, reader); 
      }
    }

    public ContentTypeReader GetTypeReader(Type type)
    {
      ContentTypeReader result;
      if (!_contentTypeMap.TryGetValue(type, out result))
      {
        // first check if the type is an array, list or dictionary if so generate dynamically
        if (type.IsArray)
        {
          result = CreateGenericTypeReader(typeof(ArrayReader<>), type.GetElementType());
        }
        else if (type.IsGenericType)
        {
          if (typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            result = CreateGenericTypeReader(typeof(ListReader<>), type.GetGenericArguments());
          else if (typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            result = CreateGenericTypeReader(typeof(DictionaryReader<,>), type.GetGenericArguments());
        }
        if (result != null)
        {
          result.Initialize(this);
          _contentTypeMap.Add(type, result);
        }
        else
          throw new KeyNotFoundException(string.Format("Type {0} has no registered ContentTypeReader", type));
      }

      return result;
    }

    public ContentTypeReader GetTypeReaderByID(Guid id)
    {
      foreach (KeyValuePair<Type, ContentTypeReader> pair in _contentTypeMap)
      {
        if (pair.Value.ID == id)
          return pair.Value;
      }
      throw new ContentLoadException(string.Format("ContentTypeReader not found with id {0}", id));
    }

    private ContentTypeReader CreateGenericTypeReader(Type genericType, Type genericArgument)
    {
      return CreateGenericTypeReader(genericType, new Type[] { genericArgument });
    }

    private ContentTypeReader CreateGenericTypeReader(Type genericType, Type[] genericArguments)
    {
      Type type = genericType.MakeGenericType(genericArguments);

      return (ContentTypeReader)Activator.CreateInstance(type);
    }

    /// <summary>
    /// Adds the inbuilt data ContentTypeReaders to the content reader
    /// </summary>
    private void AddDefaultReaders()
    {
      RegisterTypeReader(typeof(bool), new BooleanReader());
      RegisterTypeReader(typeof(byte), new ByteReader());
      RegisterTypeReader(typeof(sbyte), new SByteReader());
      RegisterTypeReader(typeof(char), new CharReader());
      RegisterTypeReader(typeof(double), new DoubleReader());
      RegisterTypeReader(typeof(float), new SingleReader());
      RegisterTypeReader(typeof(short), new Int16Reader());
      RegisterTypeReader(typeof(int), new Int32Reader());
      RegisterTypeReader(typeof(long), new Int64Reader());
      RegisterTypeReader(typeof(ushort), new UInt16Reader());
      RegisterTypeReader(typeof(uint), new UInt32Reader());
      RegisterTypeReader(typeof(ulong), new UInt64Reader());
      RegisterTypeReader(typeof(string), new StringReader());
      RegisterTypeReader(typeof(Guid), new UuidReader());
      RegisterTypeReader(typeof(object), new ExternalReferenceReader());
      RegisterTypeReader(typeof(Dictionary<object, object>), new DictionaryReader<object, object>());
      RegisterTypeReader(typeof(object[]), new ArrayReader<object>());
      RegisterTypeReader(typeof(List<object>), new ListReader<object>());
    }
  }
}

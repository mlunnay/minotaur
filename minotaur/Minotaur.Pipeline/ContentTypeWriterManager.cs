using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Minotaur.Pipeline.Writers;

namespace Minotaur.Pipeline
{
  public class ContentTypeWriterManager
  {
    private Dictionary<Type, ContentTypeWriter> _contentTypeMap = new Dictionary<Type, ContentTypeWriter>();
    private Dictionary<Type, Type> _genericTypeMap = new Dictionary<Type, Type>();

    public ContentTypeWriterManager()
    {
      AddDefaultWriters();
    }

    public void RegisterTypeWriter<T>(ContentTypeWriter writer)
    {
      RegisterTypeWriter(typeof(T), writer);
    }

    public void RegisterTypeWriter(Type type, ContentTypeWriter writer)
    {
      if (!_contentTypeMap.ContainsKey(type))
      {
        writer.Initialize(this);
        _contentTypeMap.Add(type, writer);
      }
    }

    public void RegisterGenericTypeWriter(Type contentType, Type writerType)
    {
      _genericTypeMap.Add(contentType, writerType);
    }

    public ContentTypeWriter GetTypeWriter(Type type)
    {
      ContentTypeWriter result;
      if (!_contentTypeMap.TryGetValue(type, out result))
      {
        // first check if the type is an array, list or dictionary if so generate dynamically
        if (type.IsArray)
        {
          result = CreateGenericTypeWriter(typeof(ArrayWriter<>), type.GetElementType());
        }
        else if (type.IsGenericType)
        {
          Type writerType;
          if (typeof(List<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            result = CreateGenericTypeWriter(typeof(ListWriter<>), type.GetGenericArguments());
          else if (typeof(Dictionary<,>).IsAssignableFrom(type.GetGenericTypeDefinition()))
            result = CreateGenericTypeWriter(typeof(DictionaryWriter<,>), type.GetGenericArguments());
          else if (_genericTypeMap.TryGetValue(type.GetGenericTypeDefinition(), out writerType))
            result = CreateGenericTypeWriter(writerType, type.GetGenericArguments());
        }
        if (result != null)
        {
          result.Initialize(this);
          _contentTypeMap.Add(type, result);
        }
        else
          throw new KeyNotFoundException(string.Format("Type {0} has no registered ContentTypeWriter", type));
      }

      return result;
    }

    public ContentTypeWriter GetTypeWriterByID(Guid id)
    {
      foreach (KeyValuePair<Type, ContentTypeWriter> pair in _contentTypeMap)
      {
        if (pair.Value.ID == id)
          return pair.Value;
      }
      throw new ContentException(string.Format("ContentTypeWriter not found with id {0}", id));
    }

    private ContentTypeWriter CreateGenericTypeWriter(Type genericType, Type genericArgument)
    {
      return CreateGenericTypeWriter(genericType, new Type[] { genericArgument });
    }

    private ContentTypeWriter CreateGenericTypeWriter(Type genericType, Type[] genericArguments)
    {
      Type type = genericType.MakeGenericType(genericArguments);

      return (ContentTypeWriter)Activator.CreateInstance(type);
    }

    private void AddDefaultWriters()
    {
      RegisterTypeWriter(typeof(bool), new BooleanWriter());
      RegisterTypeWriter(typeof(byte), new ByteWriter());
      RegisterTypeWriter(typeof(sbyte), new SByteWriter());
      RegisterTypeWriter(typeof(char), new CharWriter());
      RegisterTypeWriter(typeof(double), new DoubleWriter());
      RegisterTypeWriter(typeof(float), new SingleWriter());
      RegisterTypeWriter(typeof(short), new Int16Writer());
      RegisterTypeWriter(typeof(int), new Int32Writer());
      RegisterTypeWriter(typeof(long), new Int64Writer());
      RegisterTypeWriter(typeof(ushort), new UInt16Writer());
      RegisterTypeWriter(typeof(uint), new UInt32Writer());
      RegisterTypeWriter(typeof(ulong), new UInt64Writer());
      RegisterTypeWriter(typeof(string), new StringWriter());
      RegisterTypeWriter(typeof(Guid), new UuidWriter());

      // add generic writers that are built into this assembly.
      Assembly assembly = this.GetType().Assembly;
      List<Type> contentTypes = new List<Type>();
      List<Type> writerTypes = new List<Type>();
      foreach (Type type in assembly.GetExportedTypes())
      {
        if (type.IsGenericType)
        {
          if (type.GetInterface("IContentTypeWriter") != null)
          {
            writerTypes.Add(type);
            continue;
          }

          Type baseType = type;
          while ((baseType = baseType.BaseType) != null)
          {
            if (baseType == typeof(ContentItem))
            {
              contentTypes.Add(type);
            }
          }
        }
      }

      foreach (Type writerType in writerTypes)
      {
        Type contentType = contentTypes.FirstOrDefault(t => t.Name == writerType.Name.Replace("Writer", "Content"));
        if (contentType != null)
          _genericTypeMap.Add(contentType, writerType);
      }
    }
  }
}

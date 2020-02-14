using System;

namespace Minotaur.Content
{
  public abstract class ContentTypeReader
  {
    /// <summary>
    /// used to indicate that object id's should not be used when storing in arrays, lists or dictionaries.
    /// </summary>
    protected bool isPrimitiveType = false;

    public bool IsPrimitiveType { get { return isPrimitiveType; } }

    public Guid ID { get; private set; }

    public Type TargetType { get; protected set; }

    public ContentTypeReader(Guid id, Type targetType)
    {
      ID = id;
      TargetType = targetType;
    }

    public virtual void Initialize(ContentTypeReaderManager manager)
    {
    }

    public abstract object Read(ContentReader reader);
  }

  public abstract class ContentTypeReader<T> : ContentTypeReader
  {
    public ContentTypeReader(Guid id)
      : base(id, typeof(T)) { }

    public ContentTypeReader(string id)
      : base(new Guid(id), typeof(T)) { }

    public TResult Read<TResult>(ContentReader reader) where TResult : T
    {
      // Ensures that the strongly typed deserialize is called with the correct type.
      return (TResult)Read(reader);
    }
  }
}

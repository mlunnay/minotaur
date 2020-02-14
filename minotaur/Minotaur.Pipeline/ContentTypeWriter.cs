using System;

namespace Minotaur.Pipeline
{
  public abstract class ContentTypeWriter : IContentTypeWriter
  {
    /// <summary>
    /// used to indicate that object id's should not be used when storing in arrays, lists or dictionaries.
    /// </summary>
    protected bool isPrimitiveType = false;

    public bool IsPrimitiveType { get { return isPrimitiveType; } }

    public Guid ID { get; private set; }

    public Type TargetType { get; protected set; }

    public ContentTypeWriter(Guid id, Type targetType)
    {
      ID = id;
      TargetType = targetType;
    }

    public virtual void Initialize(ContentTypeWriterManager manager)
    {
    }

    public abstract void Write(ContentWriter writer, object value);

    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;

      ContentTypeWriter other = obj as ContentTypeWriter;
      if (other == null)
        return false;

      return ID == other.ID;
    }

    public bool Equals(ContentTypeWriter other)
    {
      if (other == null)
        return false;

      return ID == other.ID;
    }

    public override int GetHashCode()
    {
      return ID.GetHashCode();
    }
  }

  public abstract class ContentTypeWriter<T> : ContentTypeWriter
  {
    public ContentTypeWriter(Guid id)
      : base(id, typeof(T)) { }

    public ContentTypeWriter(string id)
      : base(new Guid(id), typeof(T)) { }

    public override void Write(ContentWriter writer, object value)
    {
      //Write(writer, (T)value);
      Write(writer, ContentTypeWriter<T>.CastType(value));
    }

    public abstract void Write(ContentWriter writer, T value);

    private static T CastType(object value)
    {
      if (value == null)
      {
        throw new ArgumentNullException("value");
      }
      if (!(value is T))
      {
        throw new ArgumentException(string.Format("Invalid argument type. Excpected {0} recieved {1}", new object[]
				{
					typeof(T),
					value.GetType()
				}));
      }
      return (T)((object)value);
    }
  }
}

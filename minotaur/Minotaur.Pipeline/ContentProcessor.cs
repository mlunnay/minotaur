using System;

namespace Minotaur.Pipeline
{
  public abstract class ContentProcessor<TInput, TOutput> : IContentProcessor
  {
    public abstract TOutput Process(TInput input, ContentProcessorContext context);

    #region IContentProcessor Members

    Type IContentProcessor.InputType
    {
      get { return typeof(TInput); }
    }

    Type IContentProcessor.OutputType
    {
      get { return typeof(TOutput); }
    }

    object IContentProcessor.Process(object input, ContentProcessorContext context)
    {
      if (input == null)
        throw new ArgumentNullException("input");
      if (context == null)
        throw new ArgumentNullException("context");
      return Process((TInput)input, context);
    }

    #endregion
  }
}

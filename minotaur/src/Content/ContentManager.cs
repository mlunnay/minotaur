using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace Minotaur.Content
{
  public class ContentManager
  {
    #region Declarations

    public enum LoadingStatus
    {
      NotLoaded,
      Loading,
      Loaded
    }

    /// <summary>
    /// The ContentItem structure stores references to already loaded content,
    /// as well as the uri so the content can be reloaded.
    /// </summary>
    public class ContentItem : IDisposable
    {
      private bool _disposed;
      public string URI;
      public IDisposable Content;
      public LoadingStatus LoadingStatus;
      internal Action<ContentItem> loader;

      public ContentItem(string uri, IDisposable content)
      {
        URI = uri;
        Content = content;
        LoadingStatus = LoadingStatus.NotLoaded;
        _disposed = false;
      }

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      private void Dispose(bool disposing)
      {
        if (_disposed)
          return;

        Content.Dispose();

        if (disposing)
        { }


        _disposed = true;
      }
    }

    /// <summary>
    /// A collection class that stores ContentItems using the URI as a key.
    /// </summary>
    private class ContentItemCollection : KeyedCollection<string, ContentItem>
    {
      public ContentItemCollection() : base() { }

      protected override string GetKeyForItem(ContentItem item)
      {
        return item.URI;
      }
    }

    private Dictionary<string, IContentLoader> _contentLoaders = new Dictionary<string, IContentLoader>();
    private IContentLoader _defaultContentLoader;
    private IServiceProvider _serviceProvider;
    private ContentItemCollection _contentItems = new ContentItemCollection();
    private ContentTypeReaderManager _typeReaderManager = new ContentTypeReaderManager();
    private ConcurrentQueue<ContentItem> _loadQueue = new ConcurrentQueue<ContentItem>();
    private Thread _loadThread;

    public readonly string RootDirectory; 

    #endregion

    #region Properties

    public ContentTypeReaderManager TypeReaderManager { get { return _typeReaderManager; } }

    public bool IsLoading { get { return _loadThread != null && _loadThread.IsAlive; } }

    #endregion

    #region Constructors

    public ContentManager(IServiceProvider servicePrivider)
      : this(servicePrivider, string.Empty) { }

    public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
    {
      if (serviceProvider == null)
        throw new ArgumentNullException("serviceProvider");
      RootDirectory = rootDirectory;
      _serviceProvider = serviceProvider;
      _defaultContentLoader = new FileContentLoader("file", rootDirectory);
      AddContentLoader(new BuiltinContentLoader());
    }

    public ContentManager(ContentManager cm)
    {
      _contentLoaders = cm._contentLoaders;
      _defaultContentLoader = cm._defaultContentLoader;
      _serviceProvider = cm._serviceProvider;
      RootDirectory = cm.RootDirectory;
    }

    #endregion

    #region Public Methods

    public void AddContentLoader(IContentLoader loader)
    {
      if (loader.Scheme == "file")
        throw new ArgumentException("Content loader cannot be registered for the scheme file");
      _contentLoaders.Add(loader.Scheme, loader);
    }

    public void RemoveContentLoader(IContentLoader loader)
    {
      _contentLoaders.Remove(loader.Scheme);
    }

    public void RemoveContentLoader(string scheme)
    {
      _contentLoaders.Remove(scheme);
    }

    public bool HasContentLoader(string scheme)
    {
      return (scheme == "file" || _contentLoaders.ContainsKey(scheme));     
    }

    public void RegisterTypeReader<T>(ContentTypeReader reader)
    {
      _typeReaderManager.RegisterTypeReader<T>(reader);
    }

    public void RegisterTypeReader(Type type, ContentTypeReader reader)
    {
      _typeReaderManager.RegisterTypeReader(type, reader);
    }

    /// <summary>
    /// Return a content item if it is already loaded, otherwise load the content item syncronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="enque"></param>
    /// <returns></returns>
    public ContentItem Get<T>(string uri, bool enque = false)
    {
      ContentItem item;
      lock (_contentItems)
      {
        if (_contentItems.Contains(uri))
          return _contentItems[uri];
        else
        {
          item = new ContentItem(uri, null);
          _contentItems.Add(item);
        }
      }
      item.loader = Loader<T>;
      if (enque)
        _loadQueue.Enqueue(item);
      else
      {
        item.LoadingStatus = LoadingStatus.Loading;
        item.loader(item);
        item.LoadingStatus = LoadingStatus.Loaded;
      }
      return item;
    }

    /// <summary>
    /// Enque a content item to load asyncronously
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    public void Enque<T>(string uri)
    {
      ContentItem item = new ContentItem(uri, null);
      item.LoadingStatus = LoadingStatus.NotLoaded;
      lock (_contentItems)
      {
        _contentItems.Add(item);
      }
      lock (_loadQueue)
      {
        item.loader = Loader<T>;
      }
    }

    /// <summary>
    /// Load the enqued content items syncronously
    /// </summary>
    public void LoadContent()
    {
      ContentItem item;
      while (_loadQueue.TryDequeue(out item))
      {
        item.loader(item);
      }
    }

    /// <summary>
    /// Load the enqued content items asyncronously
    /// </summary>
    public void LoadContentAsync()
    {
      if (_loadThread == null || !_loadThread.IsAlive)
      {
        _loadThread = new Thread(new ThreadStart(LoadingWorker));
        _loadThread.Name = "ContentManager.LoadContentAsync";
        _loadThread.Start();
      }
    }

    /// <summary>
    /// Unload all of the resources.
    /// </summary>
    public void Unload()
    {
      if (IsLoading)
        throw new InvalidOperationException("Content Manager is currently loading");
      lock (_contentItems)
      {
        foreach (ContentItem item in _contentItems)
        {
          item.Dispose();
        }
        _contentItems.Clear();
      }
    }

    /// <summary>
    /// Reload a specific content item.
    /// </summary>
    /// <param name="uri"></param>
    public void Reload(string uri)
    {
      ContentItem item;
      lock (_contentItems)
      {
        if (_contentItems.Contains(uri))
          item = _contentItems[uri];
        else
          throw new KeyNotFoundException(string.Format("Uri does not exist: {0}", uri));
      }
      item.LoadingStatus = LoadingStatus.Loading;
      item.loader(item);
      item.LoadingStatus = LoadingStatus.Loaded;
    }

    /// <summary>
    /// Reload all content items sychronously
    /// </summary>
    public void ReloadAll()
    {
      lock (_contentItems)
      {
        foreach (ContentItem item in _contentItems)
        {
          item.LoadingStatus = LoadingStatus.Loading;
          item.loader(item);
          item.LoadingStatus = LoadingStatus.Loaded;
        }
      }
    }

    /// <summary>
    /// Reload all content items asychronously
    /// </summary>
    public void ReloadAllAsync()
    {
      lock (_contentItems)
      {
        foreach (ContentItem item in _contentItems)
        {
          _loadQueue.Enqueue(item);
        }
      }

      LoadContentAsync();
    }

    #endregion

    #region Protected Methods

    protected virtual Stream GetStream(string uri) 
    {
      IContentLoader loader;
      if (uri.StartsWith("\\") || uri.StartsWith("\\\\") || uri.StartsWith("/") || uri.StartsWith("//") || !uri.Contains("://"))
      {
        loader = _defaultContentLoader;
        uri = string.Format("file://{0}", uri.TrimStart(new char[] { '\\', '/' }));
      }
      else if (!uri.Contains(":") || !_contentLoaders.TryGetValue(uri.Split(new char[] { ':' }, 2)[0], out loader))
      {
        throw new ArgumentException("URI does not have a registered loader for is scheme");
      }

      Stream stream;
      loader.HandleRequest(uri, out stream);

      return stream;
    }

    #endregion

    #region Private Methods

    private void LoadingWorker()
    {
      ContentItem item;
      while (_loadQueue.TryDequeue(out item))
      {
        item.loader(item);
      }
    }

    private void Loader<T>(ContentItem item)
    {
      item.LoadingStatus = LoadingStatus.Loading;
      Stream stream = GetStream(item.URI);
      ContentReader reader = ContentReader.Create(_typeReaderManager, this, stream);
      item.Content = reader.ReadAsset<T>() as IDisposable;
      item.LoadingStatus = LoadingStatus.Loaded;
    }

    #endregion
  }
}

/// ###TL;DR..
/// 
/// Throughout these documents, an `IEntityRecord` will be referred to as an *entity*. You can visualize it as a **row** in a database table, where each **column** holds a **component**. 

/// ####Similarities and differences
/// 
/// Just like in a regular database, it's not possible to have 2 identical rows. And, in most implementations, it's not possible to attach more 
/// than one column/component of the same type to a single entity - though I guess that actually makes it quite dissimilar.
/// 
/// But, since the components are just instances of real classes, they're not limited to being specific datatypes, and instead of simply 
/// providing data, they can have behavior too.

/// ##Source
using System;
using System.Collections.Generic;

namespace ComponentKit {
    /// <summary>
    /// Defines a single and uniquely identifiable entity.
    /// </summary>
    public interface IEntityRecord : ISynchronizable, IEquatable<IEntityRecord>, IEnumerable<IComponent> {
        /// <summary>
        /// Gets the unique name identifier for the entity.
        /// </summary>
        /// <remarks>
        /// > It is debatable whether this should be considered an implementation detail, and not be exposed at all.
        /// </remarks>
        string Name {
            get;
            set;
        }

      /// <summary>
      /// Gets the parent of this entity
      /// </summary>
        IEntityRecord Parent
        {
          get;
          set;
        }

        event EventHandler<EntityEventArgs> ParentChanged;

        /// <summary>
        /// The child entities of this entity
        /// </summary>
        List<IEntityRecord> Children
        {
            get;
        }

        /// <summary>
        /// Gets the registry that the entity is registered to (when in a synchronized state).
        /// </summary>
        IEntityRecordCollection Registry {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a component that matches the specified type name, if it is attached to the entity.
        /// </summary>
        IComponent this[string componentNameOrType] { get; }
        
        /// ###Messaging

        /// <summary>
        /// Broadcasts to all attached components with a message containing arbitrary data.
        /// </summary>
        void Broadcast<TData>(string message, TData data);

        /// <summary>
        /// Notifies attached components that have registered for the message type.
        /// </summary>
        /// <param name="message">The message to notify.</param>
        /// <param name="parameter">The parameter object to pass to the registered callbacks.</param>
        void Notify(object message, object parameter);

        /// <summary>
        /// Registers a callback for a message type.
        /// </summary>
        /// <param name="message">The message type to register.</param>
        /// <param name="callback">The callback to register.</param>
        void Register(object message, Action<object> callback);

        /// <summary>
        /// Unregister a callback for a message type.
        /// </summary>
        /// <param name="message">The message type to register.</param>
        /// <param name="callback">The callback to unregister.</param>
        void Unregister(object message, Action<object> callback);
    }
}

/// Copyright 2012 Jacob H. Hansen.
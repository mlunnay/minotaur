/// ###TL;DR..
/// 
/// A component only has one really important attribute. The parent entity.

/// ##Source
using System;

namespace ComponentKit {
    /// <summary>
    /// Defines a component of an entity.
    /// </summary>
    public interface IComponent : ISyncState, IDisposable {
        /// <summary>
        /// Gets or sets the entity that this component is attached to.
        /// </summary>
        IEntityRecord Record { get; set; }
        /// <summary>
        /// Receives a message containing arbitrary data.
        /// </summary>
        void Receive<TData>(string message, TData data);

        /// <summary>
        /// Notify this components entity with the given message and parameter.
        /// </summary>
        /// <param name="message">The message to trigger.</param>
        /// <param name="parameter">The parameter for the message.</param>
        void Notify(object message, object parameter);
    }
}

/// Copyright 2012 Jacob H. Hansen.
using System;
using System.Collections.Generic;

namespace ComponentKit.Model
{
	/// <summary>
	/// Provides loosely-coupled messaging between
	/// various colleagues.  All references to objects
	/// are stored weakly, to prevent memory leaks.
	/// </summary>
	public class Mediator
	{
		#region Fields

		readonly MessageToActionsMap _messageToCallbacksMap;

		#endregion // Fields

		#region Constructor

		public Mediator()
		{
			_messageToCallbacksMap = new MessageToActionsMap();
		}

		#endregion // Constructor

		#region Methods

		public void Register(object message, Action<object> callback)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (callback == null)
				throw new ArgumentNullException("callback");

			if (callback.Target == null)
				throw new ArgumentException("The 'callback' delegate must reference an instance method.");

			_messageToCallbacksMap.AddAction(message, callback);
		}

		public void Unregister(object message, Action<object> callback)
		{
				if (message == null)
						throw new ArgumentNullException("message");

				if (callback == null)
						throw new ArgumentNullException("callback");

				if (callback.Target == null)
						throw new ArgumentException("The 'callback' delegate must reference an instance method.");

				_messageToCallbacksMap.RemoveAction(message, callback);
		}

		public void NotifyColleagues(object message, object parameter)
		{
			List<Action<object>> actions = 
				_messageToCallbacksMap.GetActions(message);

			if (actions != null)
				actions.ForEach(action => action(parameter));
		}

		#endregion // Methods
	}
}
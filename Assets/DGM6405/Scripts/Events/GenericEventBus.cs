using System;
using System.Collections.Generic;

public class GenericEventBus
{
	private readonly Dictionary<Type, List<object>> _listenerMap = new();

	public void Register<T>(T listener)
	{
		Register(typeof(T), listener);
	}

	public void Register(Type type, object listener)
	{
		if (!_listenerMap.TryGetValue(type, out var listeners))
		{
			listeners = new List<object>();
			_listenerMap[type] = listeners;
		}

		if (!listeners.Contains(listener)) listeners.Add(listener);
	}

	public void Unregister<T>(T listener)
	{
		Unregister(typeof(T), listener);
	}

	public void Unregister(Type type, object listener)
	{
		if (_listenerMap.TryGetValue(type, out var listeners)) listeners.Remove(listener);
	}

	public void Raise<T>(Action<T> action)
	{
		var type = typeof(T);
		if (_listenerMap.TryGetValue(type, out var listeners))
			// Iterate backwards to allow unregistration during invocation
			for (var i = listeners.Count - 1; i >= 0; i--)
				if (listeners[i] is T listener)
					action(listener);
	}
}

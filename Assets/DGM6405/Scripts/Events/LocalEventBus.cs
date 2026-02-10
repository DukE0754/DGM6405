using System;
using System.Collections.Generic;
using UnityEngine;

namespace DGM6405.Events
{
	/// <summary>
	///     A local event bus for entity-specific events.
	///     Components on the same GameObject can register themselves to listen for events.
	/// </summary>
	public class LocalEventBus : MonoBehaviour
	{
		[Tooltip("Listeners automatically found on this GameObject in the Editor.")]
		[SerializeField] private List<MonoBehaviour> _bakedListeners = new();

		private readonly GenericEventBus _bus = new();

		private void Awake()
		{
			// Register all baked listeners
			foreach (var listener in _bakedListeners)
			{
				if (listener == null) continue;
				RegisterAllInterfaces(listener);
			}

			// Failsafe: Register any IEntityListener components that might have been added at runtime 
			// or weren't caught in the Editor's baked list.
			var components = GetComponents<MonoBehaviour>();
			foreach (var comp in components)
			{
				if (comp is IEntityListener && !_bakedListeners.Contains(comp))
				{
					RegisterAllInterfaces(comp);
				}
			}
		}

		private void OnValidate()
		{
			if (Application.isPlaying) return;

			_bakedListeners.Clear();
			var components = GetComponents<MonoBehaviour>();
			foreach (var comp in components)
				if (comp is IEntityListener)
					_bakedListeners.Add(comp);
		}

		private void RegisterAllInterfaces(MonoBehaviour listener)
		{
			var type = listener.GetType();
			var interfaces = type.GetInterfaces();
			foreach (var @interface in interfaces)
				if (typeof(IEntityListener).IsAssignableFrom(@interface) && @interface != typeof(IEntityListener))
					_bus.Register(@interface, listener);
		}

		public void Register<T>(T listener)
		{
			_bus.Register(listener);
		}

		public void Unregister<T>(T listener)
		{
			_bus.Unregister(listener);
		}

		public void Raise<T>(Action<T> action)
		{
			_bus.Raise(action);
		}
	}
}

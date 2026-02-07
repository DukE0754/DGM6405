using System;

namespace DGM6405.Events
{
	/// <summary>
	///     A global singleton event bus for game-wide events.
	///     Systems can register to listen for high-level state changes.
	/// </summary>
	public class GlobalEventBus : Singleton<GlobalEventBus>
	{
		private readonly GenericEventBus _bus = new();

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

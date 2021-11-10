using System;
using UnityEngine;

namespace UEventSystem
{
	public class UEvent<T>
	{
		public static event Action<T> Event;
		
		public static void Invoke(T message)
		{
			Event?.Invoke(message);
		}
	}
	
	public static class UEvent
	{
		public static void Invoke<T>(T value)
		{
			UEvent<T>.Invoke(value);
		}

		public static void Add<T>(Action<T> callback)
		{
			UEvent<T>.Event += callback;
		}

		public static void Remove<T>(Action<T> callback)
		{
			UEvent<T>.Event -= callback;
		}

		/// <summary>
		/// Bind this callback to the OnEnable() and OnDisable() events on this MonoBehaviour
		/// </summary>
		/// <param name="monoBehaviour"></param>
		/// <param name="callback"></param>
		/// <typeparam name="T"></typeparam>
		public static void Bind<T>(this MonoBehaviour monoBehaviour, Action<T> callback)
		{
			var binder = monoBehaviour.gameObject.AddComponent<EventBinder>();
			binder.onEnabled = () => Add(callback);
			binder.onDisabled = () => Remove(callback);
		}
	}
}
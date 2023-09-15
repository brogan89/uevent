using System;
using System.Collections.Generic;
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
		private static readonly Dictionary <string, List<Delegate>> _callbackMap = new();

		public static void Invoke<T>(T value)
		{
			UEvent<T>.Invoke(value);
		}
		
		public static void Invoke(string eventName)
		{
			if (!_callbackMap.ContainsKey(eventName))
				return;

			foreach (var callback in _callbackMap[eventName])
			{
				if (callback is null)
					continue;
				
				if (callback.Method.GetParameters().Length != 0)
					continue;
				
				callback.DynamicInvoke();
			}
		}
		
		public static void Invoke<T>(string eventName, T value)
		{
			if (!_callbackMap.ContainsKey(eventName))
				return;

			foreach (var callback in _callbackMap[eventName])
			{
				if (callback is null)
					continue;
				
				if (callback.Method.GetParameters().Length != 1)
					continue;
				
				callback.DynamicInvoke(value);
			}
		}

		public static void Add<T>(Action<T> callback)
		{
			UEvent<T>.Event += callback;
		}

		public static void Add(string eventName, Action callback)
		{
			if (!_callbackMap.ContainsKey(eventName))
				_callbackMap[eventName] = new List<Delegate>();
			
			_callbackMap[eventName].Add(callback);
		}
		
		public static void Add<T>(string eventName, Action<T> callback)
		{
			if (!_callbackMap.ContainsKey(eventName))
				_callbackMap[eventName] = new List<Delegate>();
			
			_callbackMap[eventName].Add(callback);
		}

		public static void Remove<T>(Action<T> callback)
		{
			UEvent<T>.Event -= callback;
		}

		public static void Remove(string eventName, Action callback)
		{
			if (_callbackMap.ContainsKey(eventName))
				_callbackMap[eventName].Add(callback);
		}
		
		public static void Remove<T>(string eventName, Action<T> callback)
		{
			if (_callbackMap.ContainsKey(eventName))
				_callbackMap[eventName].Add(callback);
		}

		/// <summary>
		/// Bind this callback to the OnEnable() and OnDisable() events on this MonoBehaviour
		/// </summary>
		/// <param name="monoBehaviour"></param>
		/// <param name="callback"></param>
		/// <typeparam name="T"></typeparam>
		public static void Bind<T>(this MonoBehaviour monoBehaviour, Action<T> callback)
		{
			CreateEventBinder(monoBehaviour,
				callback.Method.Name,
				() => Add(callback),
				() => Remove(callback));
		}

		public static void Bind(this MonoBehaviour monoBehaviour, string eventName, Action callback)
		{
			CreateEventBinder(monoBehaviour,
				callback.Method.Name,
				() => Add(eventName, callback),
				() => Remove(eventName, callback));
		}

		public static void Bind<T>(this MonoBehaviour monoBehaviour, string eventName, Action<T> callback)
		{
			CreateEventBinder(monoBehaviour,
				callback.Method.Name,
				() => Add(eventName, callback),
				() => Remove(eventName, callback));
		}

		private static void CreateEventBinder(Component monoBehaviour, string callbackMethodName, Action enabledCallback, Action disabledCallback)
		{
			var binder = monoBehaviour.gameObject.AddComponent<EventBinder>();
			binder.MethodName = callbackMethodName;
			binder.enabled = false;
			binder.onEnabled += enabledCallback;
			binder.onDisabled += disabledCallback;
			binder.enabled = true;
		}
	}
}
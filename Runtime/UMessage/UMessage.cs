// comment this out if networking not needed
#define NETWORKING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

#if NETWORKING
using MLAPI;
using MLAPI.Messaging;
#endif

namespace UMessageSystem
{
	public enum MessageType : byte
	{
		/// <summary>
		/// The message with be sent to every client
		/// </summary>
		Everyone,
		
		/// <summary>
		/// The message will only be sent to host
		/// </summary>
		HostOnly,
		
		/// <summary>
		/// Message only gets sent to the owner who published it. Same as a non-networking publish
		/// </summary>
		OwnerOnly
	}
	
	/// <summary>
	/// A simple implementation of Event Bus system
	///
	/// <para></para>
	/// This must be attached to a gameObject in a scene for the Networking side to work
	/// as MLAPI relies on NetworkBehaviour
	/// 
	/// <para></para>
	/// Limitations:
	/// - Uses UnityEngine.JsonUtilities
	/// - Uses Reflection and LINQ for Networking to find ISubscriber{T}
	/// </summary>
	public class UMessage
#if NETWORKING
		: NetworkBehaviour
#endif
	{
		/// <summary>
		/// Binding flags used for finding method with custom attribute on it
		/// </summary>
		private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
		                                   BindingFlags.NonPublic;
		
#if NETWORKING
		private static UMessage _instance;
		private static UMessage Instance =>
			_instance ? _instance : _instance = FindObjectOfType<UMessage>();
#endif

		/// <summary>
		/// List of subscribers
		/// </summary>
		private static readonly List<ISubscriber> _Subs = new List<ISubscriber>();
		
		[SerializeField] private bool _showLogs;
		private static readonly Stopwatch sw = new Stopwatch();

		#region Sub/Unsub
		
		/// <summary>
		/// Subscribe to ISubscriber callbacks
		/// </summary>
		/// <param name="sub"></param>
		public static void Sub(ISubscriber sub)
		{
			if (!_Subs.Contains(sub))
				_Subs.Add(sub);
		}
		
		/// <summary>
		/// Subscribe to ISubscriber callbacks
		/// </summary>
		/// <param name="sub"></param>
		public static void Unsub(ISubscriber sub)
		{
			_Subs.Remove(sub);
		}

		/// <summary>
		/// Bind subscriber to MonoBehaviour.
		/// This will add a script to the GameObject which in turn will sub/unsub in its OnEnable/OnDisable methods
		/// </summary>
		/// <param name="sub">Bind an ISubscriber. Only MonoBehaviour's are supported</param>
		/// <param name="destroyOnly">If true will only unsub when script is destroyed</param>
		public static void Bind(ISubscriber sub, bool destroyOnly = false)
		{
			if (!(sub is MonoBehaviour mono))
			{
				LogError($"Binding can only be done to {nameof(MonoBehaviour)}'s");
				return;
			}
			
			var binder = mono.gameObject.AddComponent<SubscriberBinder>();
			binder.DestroyOnly = destroyOnly;
			binder.Bind(mono);
		}

		#endregion

		#region Message Hanlding
		
		/// <summary>
		/// Publishes event to all subscribers
		/// </summary>
		/// <param name="eventMessage"></param>
		/// <param name="messageType"></param>
		/// <typeparam name="T"></typeparam>
		public static void Publish<T>(T eventMessage, MessageType messageType = MessageType.Everyone) where T : IMessage
		{
			if (eventMessage == null)
				throw new NullReferenceException($"{nameof(eventMessage)} is null. Publish failed");

			// ReSharper disable once SuspiciousTypeConversion.Global
			if (eventMessage is MonoBehaviour)
				throw new ArgumentException(
					$"IMessage should not be implemented by a {nameof(MonoBehaviour)}. Use custom classes only");

#if NETWORKING
			// if there is no networking manager then we default to owner only
			if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
				messageType = MessageType.OwnerOnly;
			
			if (messageType == MessageType.Everyone || messageType == MessageType.HostOnly)
			{
				if (!Instance)
				{
					LogError($"{nameof(UMessage)} Instance has been destroyed");
					return;
				}
				
				var packet = new Packet(eventMessage);
				Log($"{nameof(Publish)}. ({messageType}): {packet}");
				Instance.PublishServerRpc(packet, messageType);
			}
			else
#endif
			{
				Log($"{nameof(Publish)}. eventMessage: {eventMessage}");
				PublishOwnerOnly(eventMessage);
			}
		}
		
		/// <summary>
		/// Publish an event with data as parameter.
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="eventMessage"></param>
		/// <param name="messageType"></param>
		/// <typeparam name="T"></typeparam>
		public static void Publish<T>(string eventName, T eventMessage = default, MessageType messageType = MessageType.Everyone)
		{
			Publish(new GenericMessage<T>(eventName, eventMessage), messageType);
		}

		/// <summary>
		/// Publish an event with no data. Parameterless method callbacks only
		/// </summary>
		/// <param name="eventName"></param>
		/// <param name="messageType"></param>
		public static void Publish(string eventName, MessageType messageType = MessageType.Everyone)
		{
			Publish(new GenericMessage<object>(eventName, null), messageType);
		}

		/// <summary>
		/// Publishes event to all subscribers
		/// </summary>
		/// <param name="eventMessage"></param>
		/// <typeparam name="T"></typeparam>
		private static void PublishOwnerOnly<T>(T eventMessage) where T : IMessage
		{
			if (eventMessage == null)
				LogError($"{nameof(eventMessage)} is null. Publish failed");
			else
				InvokeMethods(eventMessage, typeof(T));
		}
		
#if NETWORKING

		[ServerRpc(RequireOwnership = false)]
		private void PublishServerRpc(Packet packet, MessageType messageType)
		{
			Log($"{nameof(PublishServerRpc)}. ({messageType}) {packet}");
			if (messageType == MessageType.HostOnly)
				MessageReceivedInternal(packet);
			else
				MessageReceivedClientRpc(packet);
		}

		// ReSharper disable once MemberCanBeMadeStatic.Local
		[ClientRpc]
		private void MessageReceivedClientRpc(Packet packet)
		{
			Log($"{nameof(MessageReceivedClientRpc)}. {packet}");
			MessageReceivedInternal(packet);
		}
		
		/// <summary>
		/// RPC message received callback
		/// </summary>
		/// <param name="packet"></param>
		private static void MessageReceivedInternal(Packet packet)
		{
			if (!packet.IsValid())
			{
				LogError($"Error with incoming packet. {packet}");
				return;
			}

			var type = Type.GetType(packet.TypeString);
			var eventMessage = JsonUtility.FromJson(packet.Data, type);

			if (eventMessage == null)
			{
				LogError($"Error deserialising packet. {packet.Data}");
				return;
			}

			InvokeMethods(eventMessage, type);
		}
#endif

		/// <summary>
		/// Dynamically invoke all the methods
		/// </summary>
		/// <param name="eventMessage"></param>
		/// <param name="type"></param>
		private static void InvokeMethods(object eventMessage, Type type)
		{
			sw.Restart();
			
			// cast as generic message so we know to check for custom generic attributes
			var gm = eventMessage as GenericMessage;

			foreach (var sub in _Subs)
			{
				foreach (var method in GetMethods(sub, type, gm?.EventName))
				{
					var value = gm != null ? gm.GetValue() : eventMessage;
					method?.Invoke(sub, value != null ? new[] { value } : null);
				}
			}
			
			Log($"{nameof(InvokeMethods)} completed. {sw.ElapsedMilliseconds}ms");
			sw.Stop();
		}
		
		/// <summary>
		/// Get the callback methods from the target subscriber
		/// </summary>
		/// <param name="subscriber"></param>
		/// <param name="type"></param>
		/// <param name="eventName"></param>
		/// <returns></returns>
		private static IEnumerable<MethodInfo> GetMethods(ISubscriber subscriber, Type type, string eventName)
		{
			// look for all MessageCallbackAttribute's
			if (eventName != null)
			{
				foreach (var methodInfo in subscriber.GetType().GetMethods(FLAGS))
				{
					foreach (var attribute in methodInfo.GetCustomAttributes<UMessageCallbackAttribute>(false))
					{
						// return if the method name is the same as the event name
						// otherwise use the custom event name set in constructor
						if (methodInfo.Name == eventName || attribute.EventName == eventName)
							yield return methodInfo;
					}
				}
			}

			// get the type of ISubscriber<T>
			// should only be one method for the interfaces
			yield return subscriber.GetType()
				.GetInterfaces()
				.SingleOrDefault(x => x.GenericTypeArguments.Length > 0 && x.GenericTypeArguments[0] == type)
				?.GetMethods()
				.FirstOrDefault();
		}

		#endregion
		
		#region Logging

		private static void Log(string message)
		{
#if NETWORKING
			if (!Instance || Instance._showLogs)
#endif
				Debug.Log($"<color=yellow>[{nameof(UMessage)}]</color> {message}");
		}
		
		private static void LogError(string message)
		{
			Debug.LogError($"<color=yellow>[{nameof(UMessage)}]</color> {message}");
		}
		
		#endregion
	}
}
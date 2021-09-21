// comment this out if networking not needed
#define NETWORKING

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#if NETWORKING
using MLAPI;
using MLAPI.Messaging;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UMessageSystem
{
	public enum MessageType
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
		
#if NETWORKING
		private static UMessage _instance;
		private static UMessage Instance =>
			_instance ? _instance : _instance = FindObjectOfType<UMessage>();
#endif

		/// <summary>
		/// List of subscribers
		/// </summary>
		private static readonly List<ISubscriber> _Subs = new List<ISubscriber>();

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
		/// <param name="sub"></param>
		/// <param name="destroyOnly">If true will only unsub when script is destroyed</param>
		public static void Bind(MonoBehaviour sub, bool destroyOnly = false)
		{
			var binder = sub.gameObject.AddComponent<SubscriberBinder>();
			binder.DestroyOnly = destroyOnly;
			binder.Bind(sub);
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
				var packet = new Packet(eventMessage);
				Log($"{nameof(Publish)}. packet: {packet}");
				Instance.PublishServerRpc(packet, messageType == MessageType.HostOnly);
			}
			else
#endif
			{
				Log($"{nameof(Publish)}. eventMessage: {eventMessage}");
				PublishOwnerOnly(eventMessage);
			}
		}
		
		/// <summary>
		/// Publishes event to all subscribers
		/// </summary>
		/// <param name="eventMessage"></param>
		/// <typeparam name="T"></typeparam>
		private static void PublishOwnerOnly<T>(T eventMessage) where T : IMessage
		{
			if (eventMessage == null)
				throw new NullReferenceException($"{nameof(eventMessage)} is null. Publish failed");

			var sw = new Stopwatch();
			sw.Start();
			
			// make a copy as subs may be removed from the OnPublished callback
			foreach (var sub in _Subs.ToArray())
			{
				switch (sub)
				{
					case MonoBehaviour m when !m:
						Unsub(sub);
						break;
					case ISubscriber<T> s:
						s.OnPublished(eventMessage);
						break;
				}
			}
			
			Log($"{nameof(PublishOwnerOnly)} completed. {sw.ElapsedMilliseconds}ms");
			sw.Stop();
		}
#if NETWORKING
		
		[ServerRpc(RequireOwnership = false)]
		private void PublishServerRpc(Packet packet, bool hostOnly)
		{
			if (hostOnly)
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
		/// 
		/// </summary>
		/// <param name="packet"></param>
		private static void MessageReceivedInternal(Packet packet)
		{
			var sw = new Stopwatch();
			sw.Start();

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
			
			// make a copy as subs may be removed from the OnPublished callback
			foreach (var sub in _Subs.ToArray())
			{
				if (sub is MonoBehaviour m && !m)
				{
					Unsub(sub);
					continue;
				}

				// get the type of ISubscriber<T>
				var t = sub.GetType()
					.GetInterfaces()
					.SingleOrDefault(x => x.GenericTypeArguments.Length > 0 && x.GenericTypeArguments[0] == type);
				
				// the ISubscriber<T> will always have its one method so this should be safe
				t?.GetMethods()[0].Invoke(sub, new[] {eventMessage});
			}

			Log($"{nameof(MessageReceivedInternal)} completed. {sw.ElapsedMilliseconds}ms");
			sw.Stop();
		}
#endif

		#endregion

		private static void Log(string message)
		{
			Debug.Log($"<color=yellow>[MessageSystem]</color> {message}");
		}
		
		private static void LogError(string message)
		{
			Debug.LogError($"<color=yellow>[MessageSystem]</color> {message}");
		}
	}
}
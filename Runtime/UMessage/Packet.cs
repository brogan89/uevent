using System;
using MLAPI.Serialization;
using UnityEngine;

namespace UMessageSystem
{
	/// <summary>
	/// Packet which is sent via MLAPI
	/// </summary>
	public class Packet : INetworkSerializable
	{
		public string TypeString;
		public string Data;

		public Packet(IMessage message)
		{
			TypeString = GetTypeString(message.GetType());
			Data = JsonUtility.ToJson(message, true);
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, true);
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(TypeString)
			       && !string.IsNullOrEmpty(Data)
			       && Data != "{}";
		}

		private static string GetTypeString(Type type)
		{
			// needs to be in this format for Type.GetType()
			// see: https://stackoverflow.com/questions/3512319/resolve-type-from-class-name-in-a-different-assembly
			var path = $"{type.FullName}, {type.Assembly}";
			Debug.Log($"Type Path: {path}");
			return path;
		}

		public void NetworkSerialize(NetworkSerializer serializer)
		{
			serializer.Serialize(ref TypeString);
			serializer.Serialize(ref Data);
		}
	}
}
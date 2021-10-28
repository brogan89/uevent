using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace UMessageSystem
{
	/// <summary>
	/// Packet which is sent via netcode
	/// </summary>
	public struct Packet : INetworkSerializable
	{
		public FixedString512Bytes TypeString;
		public FixedString4096Bytes Data;
		
		public Packet(IMessage message)
		{
			TypeString = GetTypeString(message.GetType());
			Data = JsonUtility.ToJson(message);
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this, true);
		}

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(TypeString.ToString())
			       && !string.IsNullOrEmpty(Data.ToString())
			       /*&& Data != "{}"*/;
		}

		private static string GetTypeString(Type type)
		{
			// needs to be in this format for Type.GetType()
			// see: https://stackoverflow.com/questions/3512319/resolve-type-from-class-name-in-a-different-assembly
			return $"{type.FullName}, {type.Assembly}";
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref TypeString);
			serializer.SerializeValue(ref Data);
		}
	}
}
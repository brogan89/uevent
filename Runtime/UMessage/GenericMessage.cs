using UnityEngine;

namespace UMessageSystem
{
	public abstract class GenericMessage : IMessage
	{
		public string EventName;

		public abstract object GetValue();
		
		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}
	}
	
	public sealed class GenericMessage<T> : GenericMessage
	{
		public T Value;

		public GenericMessage(string eventName, T value)
		{
			EventName = eventName;
			Value = value;
		}

		public override object GetValue()
		{
			return Value;
		}
	}
}
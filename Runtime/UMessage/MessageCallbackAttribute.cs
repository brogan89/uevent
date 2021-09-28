using System;

namespace UMessageSystem
{
	/// <summary>
	/// Attributes to be used in combination with <see cref="GenericMessage"/> callbacks
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class MessageCallbackAttribute : Attribute
	{
		public string EventName { get; }
		
		public MessageCallbackAttribute() {}

		public MessageCallbackAttribute(string eventName)
		{
			EventName = eventName;
		}
	}
}
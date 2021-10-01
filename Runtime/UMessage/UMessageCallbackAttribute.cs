using System;

namespace UMessageSystem
{
	/// <summary>
	/// Attributes to be used in combination with <see cref="GenericMessage"/> callbacks
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public class UMessageCallbackAttribute : Attribute
	{
		public string EventName { get; }
		
		public UMessageCallbackAttribute() {}

		public UMessageCallbackAttribute(string eventName)
		{
			EventName = eventName;
		}
	}
}
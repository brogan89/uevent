namespace UMessageSystem
{
	/// <summary>
	/// Base subscriber interface
	/// </summary>
	public interface ISubscriber
	{
	}
	
	/// <summary>
	/// Subscriber interface which provides the callback method
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ISubscriber<in T> : ISubscriber where T : IMessage
	{
		void OnPublished(T message);
	}
}
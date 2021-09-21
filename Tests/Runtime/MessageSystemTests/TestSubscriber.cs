using UnityEngine;

namespace UMessageSystem.Tests
{
	public class TestSubscriber : MonoBehaviour, ISubscriber<TestMessage>
	{
		public bool ReceivedMessage;
		
		void ISubscriber<TestMessage>.OnPublished(TestMessage message)
		{
			Debug.Log($"TestSubscriber::TestMethod Received: {message}", this);
			ReceivedMessage = true;
		}
	}
}
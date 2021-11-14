using UnityEngine;

namespace UEventSystem.Tests
{
	public class TestSubscriber : MonoBehaviour
	{
		public bool ReceivedMessage;

		private void Start()
		{
			this.Bind<TestEvent>(OnEventInvoked);
		}

		private void OnEventInvoked(TestEvent @event)
		{
			Debug.Log($"TestSubscriber::TestMethod Received: {@event}", this);
			ReceivedMessage = true;
		}
	}
}
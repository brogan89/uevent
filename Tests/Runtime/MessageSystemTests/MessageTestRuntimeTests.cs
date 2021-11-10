using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace UEventSystem.Tests
{
	public class MessageTestRuntimeTests
	{
		/// <summary>
		/// Tests message system without networking invoked
		/// </summary>
		/// <returns></returns>
		[UnityTest]
		public IEnumerator TestMessageSystem()
		{
			var mono = new GameObject(nameof(TestSubscriber)).AddComponent<TestSubscriber>();
			var native = new TestSub();
			
			UEvent.Invoke(TestEvent.NewMessage());
			
			yield return new WaitForSeconds(1);
			
			Assert.IsTrue(mono.ReceivedMessage);
			Assert.IsTrue(native.ReceivedMessage);
			
			Debug.Log("MessageTestRuntimeTests complete");
		}
	}

	public class TestSub
	{
		public bool ReceivedMessage;

		public TestSub()
		{
			UEvent<TestEvent>.Event += OnEventInvoked;
		}

		~TestSub()
		{
			UEvent<TestEvent>.Event -= OnEventInvoked;
		}

		private void OnEventInvoked(TestEvent @event)
		{
			Debug.Log($"TestSub::TestMessage message received: {@event}");
			ReceivedMessage = true;
		}
	}
}
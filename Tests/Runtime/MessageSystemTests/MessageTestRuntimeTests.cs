using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace UMessageSystem.Tests
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
			
			UMessage.Sub(mono);
			UMessage.Sub(native);
			
			UMessage.Publish(TestMessage.NewMessage());
			
			yield return new WaitForSeconds(1);
			
			Assert.IsTrue(mono.ReceivedMessage);
			Assert.IsTrue(native.ReceivedMessage);
			
			UMessage.Unsub(mono);
			UMessage.Unsub(native);
			
			Debug.Log("MessageTestRuntimeTests complete");
		}
	}

	public class TestSub : ISubscriber<TestMessage>
	{
		public bool ReceivedMessage;

		void ISubscriber<TestMessage>.OnPublished(TestMessage message)
		{
			Debug.Log($"TestSub::TestMessage message received: {message}");
			ReceivedMessage = true;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
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
			
			UMessage.Publish(NewMessage());
			
			yield return new WaitForSeconds(1);
			
			Assert.IsTrue(mono.ReceivedMessage);
			Assert.IsTrue(native.ReceivedMessage);
			
			UMessage.Unsub(mono);
			UMessage.Unsub(native);
			
			Debug.Log("MessageTestRuntimeTests complete");
		}

		// TODO: test when creating a network manager programmatically
		/// <summary>
		/// Tests the message system using MLAPI
		/// </summary>
		/// <returns></returns>
		// [UnityTest]
		// public IEnumerator TestNetworkMessageSystem()
		// {
		// 	var mono = new GameObject(nameof(TestSubscriber)).AddComponent<TestSubscriber>();
		// 	var native = new TestSub();
		// 	
		// 	UMessage.Sub(mono);
		// 	UMessage.Sub(native);
		// 	
		// 	UMessage.Publish(NewMessage());
		// 	
		// 	yield return new WaitForSeconds(1);
		// 	
		// 	Assert.IsTrue(mono.ReceivedMessage);
		// 	Assert.IsTrue(native.ReceivedMessage);
		// 	
		// 	UMessage.Unsub(mono);
		// 	UMessage.Unsub(native);
		// 	
		// 	Debug.Log("MessageTestRuntimeTests complete");
		// }
		
		private static TestMessage NewMessage()
		{
			var message = new TestMessage
			{
				Name = "Player 1",
				Level = 69,
				Position = new Vector2(0.69f, 4.20f),
				WayPoints = new Vector2[3],
				Numbers = new List<int>()
			};

			// generate random numbers
			for (int i = 0; i < message.WayPoints.Length; i++)
			{
				message.WayPoints[i] = new Vector2(Random.value, Random.value);
				message.Numbers.Add(Mathf.RoundToInt(Random.value * 100));
			}

			return message;
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
using System.Collections.Generic;
using MLAPI;
using UnityEngine;
using UnityEngine.UI;

namespace UMessageSystem.Samples
{
	public class UMessageDemo : MonoBehaviour,
		ISubscriber<TestMessage>
	{
		public Text text;

		private const string CUSTOM_EVENT_NAME = "Another Event";
		
		private void Start()
		{
			NetworkManager.Singleton.StartHost();
			UMessage.Bind(this);
		}

		public void Publish()
		{
			UMessage.Publish(TestMessage.NewMessage());
			UMessage.Publish(nameof(ParameterlessCallback));
			UMessage.Publish(nameof(ParameterCallback), Mathf.RoundToInt(Random.value * 100));
			UMessage.Publish(CUSTOM_EVENT_NAME, 9999);
		}

		private void SetText(string value)
		{
			Debug.Log($"SetText: {value}", this);
			text.text += value + "\n";
		}

		[MessageCallbackAttribute]
		private void ParameterlessCallback()
		{
			SetText(nameof(ParameterlessCallback));
		}
		
		[MessageCallbackAttribute(nameof(ParameterlessCallback))]
		private void ParameterlessRandomMethodName()
		{
			SetText(nameof(ParameterlessRandomMethodName));
		}
		
		[MessageCallbackAttribute]
		private void ParameterCallback(int value)
		{
			SetText($"{nameof(ParameterCallback)}: {value}");
		}
		
		[MessageCallbackAttribute(nameof(ParameterCallback))]
		[MessageCallbackAttribute(CUSTOM_EVENT_NAME)]
		private void RandomMethodName(int value)
		{
			SetText($"{nameof(RandomMethodName)}: {value}");
		}

		void ISubscriber<TestMessage>.OnPublished(TestMessage message)
		{
			SetText(message.ToString());
		}
	}

	public class TestMessage : IMessage
	{
		public string Name;
		public int Level;
		public Vector2 Position;
		public Vector2[] WayPoints;
		public List<int> Numbers;

		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}
		
		public static TestMessage NewMessage()
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
}
using System.Collections.Generic;
using UnityEngine;

namespace UMessageSystem.Tests
{
	public class TestMessage : IMessage
	{
		public string Name;
		public int Level;
		public Vector2 Position;
		public Vector2[] WayPoints;
		public List<int> Numbers;

		public override string ToString()
		{
			return JsonUtility.ToJson(this, true);
		}
	}
}
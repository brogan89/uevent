using System;
using UnityEngine;

namespace UEventSystem
{
	public class EventBinder : MonoBehaviour
	{
		public Action onEnabled;
		public Action onDisabled;
		
		private void OnEnable()
		{
			onEnabled?.Invoke();
		}

		private void OnDisable()
		{
			onDisabled?.Invoke();
		}
	}
}
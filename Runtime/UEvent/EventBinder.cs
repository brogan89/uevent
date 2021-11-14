using System;
using Unity.Collections;
using UnityEngine;

namespace UEventSystem
{
	public class EventBinder : MonoBehaviour
	{
		[ReadOnly] public string MethodName;
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
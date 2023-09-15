using UnityEngine;

namespace UEventSystem
{
	public class UEventSender : MonoBehaviour
	{
		public string EventName;
		public Arg[] Args;

		[ContextMenu("Send")]
		public void Send()
		{
			if (Args.Length > 0)
			{
				var args = new Args(Args);
				UEvent.Invoke(EventName, args);
			}
			
			UEvent.Invoke(EventName);
		}
	}
}
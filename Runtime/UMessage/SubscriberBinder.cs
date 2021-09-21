using UnityEngine;

namespace UMessageSystem
{
	public class SubscriberBinder : MonoBehaviour
	{
		[Tooltip("If true will only unsub when script is destroyed")]
		public bool DestroyOnly;
		private MonoBehaviour _sub;

		private void OnEnable()
		{
			Sub();
		}

		private void OnDisable()
		{
			if (!DestroyOnly)
				Unsub();
		}

		private void OnDestroy()
		{
			if (DestroyOnly)
				Unsub();
		}

		public void Bind(MonoBehaviour sub)
		{
			_sub = sub;
			Sub();
		}

		private void Sub()
		{
			if (_sub)
				UMessage.Sub(_sub as ISubscriber);
		}

		private void Unsub()
		{
			UMessage.Unsub(_sub as ISubscriber);
		}
	}
}
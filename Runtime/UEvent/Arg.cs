using System;
using UnityEngine;

namespace UEventSystem
{
	[Serializable]
	public struct Arg
	{
		public string key;
		[SerializeField] public string _value;
		public ValueType type;
		
		public bool IsFlag => type is ValueType.Bool;
		
		public enum ValueType
		{
			String, Number, Bool
		}

		public object ConvertValue()
		{
			return type switch
			{
				ValueType.String => _value,
				ValueType.Number => float.Parse(_value),
				ValueType.Bool => true,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
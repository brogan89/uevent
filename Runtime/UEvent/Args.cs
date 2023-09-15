namespace UEventSystem
{
	public class Args
	{
		private readonly Arg[] _args;

		public Args(params Arg[] args)
		{
			_args = new Arg[args.Length];
			
			for (int i = 0; i < args.Length; i++)
			{
				_args[i] = args[i];
			}
		}

		public bool TryGetValue(string key, out object value)
		{
			foreach (var VARIABLE in _args)
			{
				if (VARIABLE.key != key) 
					continue;
				
				value = VARIABLE.ConvertValue();
				return true;
			}

			value = null;
			return false;
		}
	}
}
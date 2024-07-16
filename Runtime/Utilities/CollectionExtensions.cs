using System.Collections.Generic;
using System;

namespace Oscilloscope.Utilities
{
	public static class CollectionExtensions
	{
		
		public static Value GetOrAdd<Key, Value>(this Dictionary<Key, Value> dictionary, Key key, Func<Key, Value> factory)
		{
			if (dictionary.TryGetValue(key, out var value))
				return value;
			
			value = factory(key);
			dictionary.Add(key, value);
			return value;
		}

	}
}
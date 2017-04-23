using System.Linq;
using System.Collections.Generic;

namespace Scigeobib
{
	public static class CollectionUtils
	{
		public static int Increment<KeyType>(IDictionary<KeyType, int> dict, KeyType key)
		{
			if (dict.ContainsKey(key))
			{
				int newValue = ++dict[key];
				return newValue;
			}
			else
			{
				dict.Add(key, 1);
				return 1;
			}
		}

		public static int AddToSet<KeyType, SetItemType>(IDictionary<KeyType, HashSet<SetItemType>> dict, KeyType key, SetItemType newItem)
		{
			if (dict.ContainsKey(key))
			{
				dict[key].Add(newItem);
				return dict[key].Count;
			}
			else
			{
				HashSet<SetItemType> newSet = new HashSet<SetItemType>();
				newSet.Add(newItem);
				dict.Add(key, newSet);
				return 1;
			}
		}

		public static IDictionary<KeyType, int> SetDictToCountDict<KeyType, SetItemType>(IDictionary<KeyType, HashSet<SetItemType>> dict)
		{
			return dict.ToDictionary(kv => kv.Key, kv => kv.Value.Count);
		}
	}
}

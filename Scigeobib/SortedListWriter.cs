
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Scigeobib
{
	public class SortedListWriter
	{
		class Entry
		{
			public string name;
			public int value;
		}

		private List<Entry> entries = new List<Entry>();

		public void Add(string name, int value)
		{
			entries.Add(new Entry() { name = name, value = value});
		}

		public void Add<ItemType>(IDictionary<ItemType, int> dict, Func<ItemType, string> itemTypeToString)
		{
			foreach (var kv in dict)
				Add(itemTypeToString(kv.Key), kv.Value);
		}

		public void WriteToFile(string path)
		{
			entries.Sort((x, y) => y.value - x.value);

			using (TextWriter textWriter = new StreamWriter(path, false, Encoding.UTF8))
			{
				foreach (Entry entry in entries)
				{
					textWriter.WriteLine(entry.value + "\t" + entry.name);
				}
			}
		}
	}
}

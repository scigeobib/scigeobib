using System.Collections.Generic;

namespace Scigeobib
{
	public class PublicationWithErrors
	{
		public Publication publication;

		public List<string> unresolved = new List<string>();
		public List<string> more = new List<string>();
		public List<string> self = new List<string>();

		public bool HasErrors()
		{
			return unresolved.Count > 0 || more.Count > 0 || self.Count > 0;
		}
	}
}

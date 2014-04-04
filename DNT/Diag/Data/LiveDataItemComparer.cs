using System;
using System.Collections.Generic;

namespace DNT.Diag.Data
{
	public class LiveDataItemComparer : IComparer<LiveDataItem>
	{
		public int Compare(LiveDataItem x, LiveDataItem y)
		{
			if (x == null)
				throw new ArgumentNullException ("x");
			if (y == null)
				throw new ArgumentNullException ("y");

			return x.IndexForSort.CompareTo (y.IndexForSort);
		}
	}
}


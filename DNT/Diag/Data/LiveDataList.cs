using System;
using System.Collections;
using System.Collections.Generic;

namespace DNT.Diag.Data
{
	public class LiveDataList : IEnumerable<LiveDataItem>
	{
		private List<LiveDataItem> items;
		private Dictionary<string, LiveDataItem> queryByShortName;
		private List<LiveDataItem> needs;
		private Dictionary<string, LiveDataBuffer> bufferMap;
		private Dictionary<string, byte[]> commandNeed;
		private LiveDataItemComparer comparer;

		public LiveDataList ()
		{
			items = new List<LiveDataItem> ();
			queryByShortName = new Dictionary<string, LiveDataItem> ();
			needs = new List<LiveDataItem> ();
			bufferMap = new Dictionary<string, LiveDataBuffer> ();
			commandNeed = new Dictionary<string, byte[]> ();
			comparer = new LiveDataItemComparer ();
		}

		public IEnumerator<LiveDataItem> GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return items.GetEnumerator ();
		}

		public void Add(LiveDataItem item)
		{
			items.Add (item);
			queryByShortName.Add (item.ShortName, item);
			string key = item.CmdClass + item.CmdName;
			if (!bufferMap.ContainsKey (key))
				bufferMap.Add (key, new LiveDataBuffer ());
			item.EcuResponseBuff = bufferMap [key];
		}

		public LiveDataItem this[int index]
		{
			get { return items[index]; }
		}

		public LiveDataItem this[string shortName]
		{
			get { return queryByShortName [shortName]; }
		}

		public int Count
		{
			get { return items.Count; }
		}

		public void MakeDisplayItems()
		{
			needs.Clear ();
			foreach (var item in items) {
				if (item.IsEnabled && item.IsDisplay) {
					needs.Add (item);
					string key = item.CmdClass + item.CmdName;
					if (!commandNeed.ContainsKey (key))
						commandNeed.Add (key, item.FormattedCommand);
				}
			}

			needs.Sort (comparer);
		}

		public List<LiveDataItem> DisplayItems
		{
			get { return needs; }
		}

		public Dictionary<string, byte[]> CommandNeed
		{
			get { return commandNeed; }
		}

		public LiveDataBuffer GetMsgBuffer(string key)
		{
			return bufferMap [key];
		}
	}
}


using System;
using System.ComponentModel;

namespace DNT.Diag.Data
{
	public class LiveDataItem : INotifyPropertyChanged
	{
		private string shortName;
		private string content;
		private string unit;
		private string defaultValue;
		private string description;
		private string minValue;
		private string maxValue;
		private string cmdName;
		private string cmdClass;
		private string value;
		private byte[] command;
		private byte[] formattedCommand;
		private LiveDataBuffer ecuResponseBuff;
		private int indexForSort;
		private int position;
		private bool isEnabled;
		private bool isDisplay;
		private bool isOutOfRange;

		public Func<LiveDataItem, string> CalcFunction;
		public event PropertyChangedEventHandler PropertyChanged;

		public LiveDataItem(string shortName,
			string content,
			string unit,
			string defaultValue,
			string description,
			string cmdName,
			string cmdClass,
			byte[] command,
			int indexForSort)
		{
			this.shortName = shortName;
			this.content = content;
			this.unit = unit;
			this.defaultValue = defaultValue;
			this.description = description;
			this.minValue = "";
			this.MaxValue = "";
			this.cmdName = cmdName;
			this.cmdClass = cmdClass;
			this.value = "";
			this.command = command;
			this.formattedCommand = null;
			this.ecuResponseBuff = null;
			this.indexForSort = indexForSort;
			this.position = -1;
			this.isEnabled = false;
			this.isDisplay = false;
			this.isOutOfRange = false;
			CalcFunction = null;
			PropertyChanged = null;
		}

		public LiveDataItem ()
			: this ("", "", "", "", "", "", "", null, -1)
		{
		}

		public string ShortName {
			get {
				return shortName;
			}
			internal set {
				shortName = value;
			}
		}

		public string Content {
			get {
				return content;
			}
			internal set {
				content = value;
			}
		}

		public string Unit {
			get {
				return unit;
			}
			internal set {
				unit = value;
			}
		}

		public string DefaultValue {
			get {
				return defaultValue;
			}
			internal set {
				defaultValue = value;
			}
		}

		public string Description {
			get {
				return description;
			}
			internal set {
				description = value;
			}
		}

		public string MinValue {
			get {
				return minValue;
			}
			internal set {
				minValue = value;
			}
		}

		public string MaxValue {
			get {
				return maxValue;
			}
			internal set {
				maxValue = value;
			}
		}

		public string CmdName {
			get {
				return cmdName;
			}
			internal set {
				cmdName = value;
			}
		}

		public string CmdClass {
			get {
				return cmdClass;
			}
			internal set {
				cmdClass = value;
			}
		}

		public string Value {
			get {
				return value;
			}
			private set {
				if (this.value != value) {
					this.value = value;
					if (PropertyChanged != null)
						PropertyChanged (this, new PropertyChangedEventArgs ("Value"));
				}
			}
		}

		public byte[] Command {
			get {
				return command;
			}
			internal set {
				command = value;
			}
		}

		public byte[] FormattedCommand {
			get {
				return formattedCommand;
			}
			set {
				formattedCommand = value;
			}
		}

		public LiveDataBuffer EcuResponseBuff {
			get {
				return ecuResponseBuff;
			}
			internal set {
				ecuResponseBuff = value;
			}
		}

		public int IndexForSort {
			get {
				return indexForSort;
			}
			internal set {
				indexForSort = value;
			}
		}

		public int Position {
			get {
				return position;
			}
			set {
				position = value;
			}
		}

		public bool IsEnabled {
			get {
				return isEnabled;
			}
			set {
				isEnabled = value;
			}
		}

		public bool IsDisplay {
			get {
				return isDisplay;
			}
			set {
				isDisplay = value;
			}
		}

		public bool IsOutOfRange {
			get {
				return isOutOfRange;
			}
			private set {
				isOutOfRange = value;
			}
		}

		public void CalcValue()
		{
			if (CalcFunction != null)
				Value = CalcFunction (this);
		}
	}
}


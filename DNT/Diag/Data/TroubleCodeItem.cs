using System;

namespace DNT.Diag.Data
{
	public class TroubleCodeItem
	{
		private string code;
		private string content;
		private string description;

		public TroubleCodeItem(string code,
			string content,
			string description)
		{
			this.code = code;
			this.content = content;
			this.description = description;
		}

		public TroubleCodeItem ()
			: this("", "", "")
		{

		}

		public string Code
		{
			get { return code; }
			set { code = value; }
		}

		public string Content
		{
			get { return content; }
			set { content = value; }
		}

		public string Description
		{
			get { return description; }
			set { description = value; }
		}
	}
}


using System;

namespace DNT.Diag
{
	public static class Settings
	{
		private static Language lang;
		private static string langText;
		public static Language Language
		{
			get { return lang; }
			set {
				lang = value;
				langText = ToString (lang);
			}
		}

		public static string ToString(Language value)
		{
			return value.ToString ().Replace ('_', '-');
		}

		public static string LanguageText
		{
			get { return langText; }
		}
	}
}


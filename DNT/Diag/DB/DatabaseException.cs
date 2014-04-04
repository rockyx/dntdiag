using System;

namespace DNT.Diag.DB
{
	public class DatabaseException : Exception
	{
		public DatabaseException (string message)
			: base(message)
		{
		}
	}
}


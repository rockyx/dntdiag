using System;

namespace DNT.Diag.DB
{
	public class CryptoException : Exception
	{
		public CryptoException (string message)
			: base(message)
		{
		}
	}
}


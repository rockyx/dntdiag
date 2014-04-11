using System;

namespace DNT.Diag.Commbox
{
    public class CommboxException : Exception
    {
        public CommboxException(string message)
			: base(message)
        {
        }
    }
}


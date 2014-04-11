using System;

namespace DNT.Diag.Formats
{
    public class FormatException : Exception
    {
        public FormatException()
        {
        }

        public FormatException(string message)
			: base(message)
        {
        }
    }
}


using System;

namespace DNT.Diag.ECU
{
    public class DiagException : Exception
    {
        public DiagException()
        {
        }

        public DiagException(string msg)
			: base(msg)
        {
        }
    }
}


using System;

namespace DNT.Diag.Channel
{
    public class ChannelException : Exception
    {
        public ChannelException()
        {
        }

        public ChannelException(string msg)
			: base(msg)
        {
        }
    }
}


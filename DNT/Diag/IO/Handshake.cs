//
// System.IO.Ports.Handshake.cs
//
// Authors:
//  Chris Toshok (toshok@ximian.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

#if __ANDROID__

namespace System.IO.Ports
{
    public enum Handshake
    {
        None,
        XOnXOff,
        RequestToSend,
        RequestToSendXOnXOff
    }
}

#endif


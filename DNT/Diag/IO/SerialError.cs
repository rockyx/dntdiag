﻿//
// System.IO.Ports.SerialError.cs
//
// Authors:
//  Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

#if __ANDROID__

namespace System.IO.Ports
{
    public enum SerialError
    {
        RXOver = 1,
        Overrun = 2,
        RXParity = 4,
        Frame = 8,
        TXFull = 256
    }
}

#endif


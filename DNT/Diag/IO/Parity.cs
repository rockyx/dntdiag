//
// System.IO.Ports.Parity.cs
//
// Authors:
//  Chris Toshok (toshok@ximian.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

#if __ANDROID__

namespace System.IO.Ports
{
	public enum Parity
	{
		None,
		Odd,
		Even,
		Mark,
		Space
	}
}

#endif


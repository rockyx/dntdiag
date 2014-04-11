using System;
using System.IO.Ports;

namespace DNT.Diag.IO
{
    public static class SerialPortExtensions
    {
        public static void Write(this SerialPort port, params byte[] bs)
        {
            port.Write(bs, 0, bs.Length);
        }
    }
}


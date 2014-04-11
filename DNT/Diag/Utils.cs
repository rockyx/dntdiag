using System;

namespace DNT.Diag
{
    public static class Utils
    {
        public static byte LoByte<T>(T value)
        {
            ushort v = Convert.ToUInt16(value);
            return (byte)(v & 0x00FF);
        }

        public static byte HiByte<T>(T value)
        {
            ushort v = Convert.ToUInt16(value);
            return (byte)((v & 0xFF00) >> 8);
        }

        public static ushort LoWord<T>(T value)
        {
            uint v = Convert.ToUInt32(value);
            return (ushort)(v & 0x0000FFFF);
        }

        public static ushort HiWord<T>(T value)
        {
            uint v = Convert.ToUInt32(value);
            return (ushort)((v & 0xFFFF0000) >> 16);
        }
    }
}


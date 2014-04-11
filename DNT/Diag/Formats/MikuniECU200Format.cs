using System;
using DNT.Diag.Attributes;

namespace DNT.Diag.Formats
{
    public class MikuniECU200Format : AbstractFormat
    {
        public const byte HEAD_FORMAT = 0x48;

        public MikuniECU200Format(Parameter param)
			: base(param)
        {
        }

        public override int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int temp = dOffset;
            dest[dOffset++] = HEAD_FORMAT;
            Array.Copy(src, sOffset, dest, dOffset, count);
            dOffset += count;
            dest[dOffset++] = 0x0D;
            dest[dOffset++] = 0x0A;

            return dOffset - temp;
        }

        public override int ExpectPackLength(byte[] src, int offset, int count)
        {
            return count + 3;
        }

        public override int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            Array.Copy(src, sOffset + 1, dest, dOffset, count - 3);
            return count - 3;
        }

        public override int ExpectUnpackLength(byte[] src, int offset, int count)
        {
            return count - 3;
        }
    }
}


using System;
using DNT.Diag.Attributes;

namespace DNT.Diag.Formats
{
    public class MikuniECU300Format : AbstractFormat
    {
        public MikuniECU300Format(Parameter param)
			: base(param)
        {
        }

        public override int ExpectPackLength(byte[] src, int offset, int count)
        {
            return count + 3;
        }

        public override int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int temp = dOffset;
            int cs = 0;
            dest[dOffset] = Utils.HiByte(count + 1);
            cs += dest[dOffset++];

            dest[dOffset] = Utils.LoByte(count + 1);
            cs += dest[dOffset++];

            Array.Copy(src, sOffset, dest, dOffset, count);
            for (int i = 0; i < count; i++)
                cs += src[sOffset + i];
            dOffset += count;
            dest[dOffset++] = Utils.LoByte(cs);
            return dOffset - temp;
        }

        public override int ExpectUnpackLength(byte[] src, int offset, int count)
        {
            return count - 3;
        }

        public override int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int length = ((src[sOffset] & 0xFF) << 8) | (src[sOffset + 1] & 0xFF) - 1;
            Array.Copy(src, sOffset + 2, dest, dOffset, length);
            return length;
        }
    }
}


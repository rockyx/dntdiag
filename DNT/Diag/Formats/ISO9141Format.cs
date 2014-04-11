using System;
using DNT.Diag.Attributes;

namespace DNT.Diag.Formats
{
    public class ISO9141Format : AbstractFormat
    {
        public const int MAX_FRAME_LENGTH = 11;
        public const int MAX_DATA_LENGTH = 7;
        public const int MAX_FORMAT_LENGTH = 4;

        public ISO9141Format(Parameter param)
			: base(param)
        {
        }

        public override int ExpectPackLength(byte[] src, int offset, int count)
        {
            return count + 4;
        }

        public override int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int checksum = 0;
            int temp = dOffset;
            dest[dOffset++] = Utils.LoByte(Parameter.ISOHeader);
            checksum += Utils.LoByte(Parameter.ISOHeader);

            dest[dOffset++] = Utils.LoByte(Parameter.KLineTargetAddress);
            checksum += Utils.LoByte(Parameter.KLineTargetAddress);

            dest[dOffset++] = Utils.LoByte(Parameter.KLineSourceAddress);
            checksum += Utils.LoByte(Parameter.KLineSourceAddress);

            Array.Copy(src, sOffset, dest, dOffset, count);
            for (int i = sOffset; i < count; i++)
                checksum += src[i];
            dOffset += count;

            dest[dOffset++] = Utils.LoByte(checksum);
            return dOffset - temp;
        }

        private int SinglePack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            count--; // data length.
            int checksum = 0;
            for (int i = sOffset; i < count; i++)
            {
                checksum += src[i];
            }
            if (checksum != src[count + sOffset])
                throw new FormatException("ISO9141 checksum error!");

            count -= 3;
            Array.Copy(src, sOffset + 3, dest, dOffset, count);
            return count;
        }

        private int CalcUnpackFrameCount(int count)
        {
            int tail = count % MAX_FRAME_LENGTH;
            return (count / MAX_FRAME_LENGTH) * MAX_DATA_LENGTH + (tail == 0 ? 0 : tail - MAX_FORMAT_LENGTH);
        }

        public override int ExpectUnpackLength(byte[] src, int offset, int count)
        {
            if (count < 5)
                throw new FormatException("ISO9141 data length error!");

            return CalcUnpackFrameCount(count);
        }

        public override int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            if (count < 5)
                return -1;

            int retlen = 0;
            int frameCount = CalcUnpackFrameCount(count);
            int tailLength = count % MAX_FRAME_LENGTH;

            for (int i = 0; i < frameCount; i++)
            {
                retlen += SinglePack(src, 
                    sOffset, 
                    dest, 
                    dOffset, 
                    i == (frameCount - 1) ? 
					(tailLength == 0 ? MAX_DATA_LENGTH : tailLength - MAX_FORMAT_LENGTH) : 
					MAX_DATA_LENGTH);

                sOffset += MAX_FRAME_LENGTH;
                dOffset += MAX_DATA_LENGTH;
            }
            return retlen;
        }
    }
}


using System;
using DNT.Diag.Attributes;

namespace DNT.Diag.Formats
{
    public class KWP2KFormat : AbstractFormat
    {
        public const int KWP8X_HEADER_LENGTH = 3;
        public const int KWPCX_HEADER_LENGTH = 3;
        public const int KWP80_HEADER_LENGTH = 4;
        public const int KWPXX_HEADER_LENGTH = 1;
        public const int KWP00_HEADER_LENGTH = 2;
        public const int KWP_CHECKSUM_LENGTH = 1;
        public const int KWP_MAX_DATA_LENGTH = 128;

        public KWP2KFormat(Parameter param)
			: base(param)
        {
        }

        public override int ExpectPackLength(byte[] src, int offset, int count)
        {
            switch (Parameter.KWP2KCurrentMode)
            {
                case KWP2KMode.Mode8X:
                    return KWP8X_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH;
                case KWP2KMode.ModeCX:
                    return KWPCX_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH;
                case KWP2KMode.Mode80:
                    return KWP80_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH;
                case KWP2KMode.Mode00:
                    return KWP00_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH;
                case KWP2KMode.ModeXX:
                    return KWPXX_HEADER_LENGTH + count + KWP_CHECKSUM_LENGTH;
            }
            return 0;
        }

        public override int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int temp = dOffset;
            int cs = 0;

            switch (Parameter.KWP2KCurrentMode)
            {
                case KWP2KMode.Mode8X:
                    dest[dOffset] = Utils.LoByte(0x80 | count);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineTargetAddress);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineSourceAddress);
                    cs += dest[dOffset++];
                    break;
                case KWP2KMode.ModeCX:
                    dest[dOffset] = Utils.LoByte(0xC0 | count);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineTargetAddress);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineSourceAddress);
                    cs += dest[dOffset++];
                    break;
                case KWP2KMode.Mode80:
                    dest[dOffset] = Utils.LoByte(0x80);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineTargetAddress);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(Parameter.KLineSourceAddress);
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(count);
                    cs += dest[dOffset++];
                    break;
                case KWP2KMode.Mode00:
                    dest[dOffset] = 0x00;
                    cs += dest[dOffset++];
                    dest[dOffset] = Utils.LoByte(count);
                    cs += dest[dOffset++];
                    break;
                case KWP2KMode.ModeXX:
                    dest[dOffset] = Utils.LoByte(count);
                    cs += dest[dOffset++];
                    break;
            }

            Array.Copy(src, sOffset, dest, dOffset, count);
            for (int i = 0; i < count; i++)
                cs += src[sOffset + i];

            dest[dOffset++] = Utils.LoByte(cs);
            return dOffset - temp;
        }

        public override int ExpectUnpackLength(byte[] src, int offset, int count)
        {
            int length = 0;

            if ((src[offset] & 0xFF) > 0x80)
            {
                length = (src[offset] & 0xFF) - 0x80;
                if ((src[offset + 1] & 0xFF) != Parameter.KLineSourceAddress)
                    throw new FormatException("KWP2K address not the same as parameter.");

                if (length != (count - KWP8X_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                {
                    length = (src[offset] & 0xFF) - 0xC0; // for kwp cx
                    if (length != (count - KWPCX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                        throw new FormatException("KWP2K length data error!");
                }
            }
            else if ((src[offset] & 0xFF) == 0x80)
            {
                length = src[offset + 3] & 0xFF;
                if ((src[offset + 1] & 0xFF) != Parameter.KLineSourceAddress)
                    throw new FormatException("KWP2K address not the same as parameter.");

                if (length != (count - KWP80_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
            }
            else if (src[offset] == 0x00)
            {
                length = src[offset + 1] & 0xFF;
                if (length != (count - KWP00_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
            }
            else
            {
                length = src[offset] & 0xFF;
                if (length != (count - KWPXX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
            }

            return length;
        }

        public override int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count)
        {
            int length = 0;

            if ((src[sOffset] & 0xFF) > 0x80)
            {
                length = (src[sOffset] & 0xFF) - 0x80;
                if ((src[sOffset + 1] & 0xFF) != Parameter.KLineSourceAddress)
                    throw new FormatException("KWP2K address not the same as parameter.");

                if (length != (count - KWP8X_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                {
                    length = (src[dOffset] & 0xFF) - 0xC0; // for kwp cx
                    if (length != (count - KWPCX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                        throw new FormatException("KWP2K length data error!");
                    else
                        sOffset += KWPCX_HEADER_LENGTH;
                }
                else
                {
                    sOffset += KWP8X_HEADER_LENGTH;
                }
            }
            else if ((src[sOffset] & 0xFF) == 0x80)
            {
                length = src[sOffset + 3] & 0xFF;
                if ((src[sOffset + 1] & 0xFF) != Parameter.KLineSourceAddress)
                    throw new FormatException("KWP2K address not the same as parameter.");

                if (length != (count - KWP80_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
                sOffset += KWP80_HEADER_LENGTH;
            }
            else if (src[sOffset] == 0x00)
            {
                length = src[sOffset + 1] & 0xFF;
                if (length != (count - KWP00_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
                sOffset += KWP00_HEADER_LENGTH;
            }
            else
            {
                length = src[sOffset] & 0xFF;
                if (length != (count - KWPXX_HEADER_LENGTH - KWP_CHECKSUM_LENGTH))
                    throw new FormatException("KWP2K length data error!");
                sOffset += KWPXX_HEADER_LENGTH;
            }

            Array.Copy(src, sOffset, dest, dOffset, length);
            return length;
        }
    }
}


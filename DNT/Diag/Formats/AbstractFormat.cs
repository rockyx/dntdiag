using System;
using DNT.Diag.Attributes;

namespace DNT.Diag.Formats
{
    public abstract class AbstractFormat : IFormat
    {
        private Parameter param;

        public AbstractFormat(Parameter param)
        {
            this.param = param;
        }

        protected Parameter Parameter
        {
            get { return param; }
        }

        public abstract int ExpectPackLength(byte[] src, int offset, int count);

        public abstract int ExpectUnpackLength(byte[] src, int offset, int count);

        public abstract int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count);

        public abstract int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count);

        public byte[] Pack(params byte[] bs)
        {
            return Pack(bs, 0, bs.Length);
        }

        public byte[] Pack(byte[] src, int offset, int count)
        {
            byte[] result = new byte[ExpectPackLength(src, offset, count)];
            Pack(src, offset, result, 0, count);
            return result;
        }

        public byte[] Unpack(byte[] src, int offset, int count)
        {
            byte[] result = new byte[ExpectUnpackLength(src, offset, count)];
            Unpack(src, offset, result, 0, count);
            return result;
        }

        public byte[] Unpack(params byte[] bs)
        {
            return Unpack(bs, 0, bs.Length);
        }
    }
}


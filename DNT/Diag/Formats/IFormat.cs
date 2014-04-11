using System;

namespace DNT.Diag.Formats
{
    public interface IFormat
    {
        byte[] Pack(byte[] src, int offset, int count);

        int Pack(byte[] src, int sOffset, byte[] dest, int dOffset, int count);

        byte[] Pack(params byte[] src);

        byte[] Unpack(byte[] src, int offset, int count);

        int Unpack(byte[] src, int sOffset, byte[] dest, int dOffset, int count);

        byte[] Unpack(params byte[] src);

        int ExpectPackLength(byte[] src, int offset, int count);

        int ExpectUnpackLength(byte[] src, int offset, int count);
    }
}


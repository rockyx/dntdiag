using System;

namespace DNT.Diag.Channel
{
    public interface IChannel
    {
        void StartHeartbeat(byte[] data, int offset, int count);

        void StopHeartbeat();

        Timer ByteTxInterval { get; set; }

        Timer FrameTxInterval { get; set; }

        Timer ByteRxTimeout { get; set; }

        Timer FrameRxTimeout { get; set; }

        Timer HeartbeatInterval { get; set; }

        int SendAndRecv(byte[] sData, int sOffset, int sCount, byte[] output);

        void StartCommunicate();
    }
}


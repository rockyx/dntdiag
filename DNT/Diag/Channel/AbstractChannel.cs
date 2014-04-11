using System;
using DNT.Diag.Attributes;
using DNT.Diag.Commbox;

namespace DNT.Diag.Channel
{
    public abstract class AbstractChannel : IChannel
    {
        private Parameter param;
        private Commbox.ICommbox commbox;
        private Timer byteTxInterval;
        private Timer frameTxInterval;
        private Timer byteRxTimeout;
        private Timer frameRxTimeout;
        private Timer heartbeatInterval;

        public AbstractChannel(Parameter param, ICommbox commbox)
        {
            this.param = param;
            this.commbox = commbox;
        }

        protected ICommbox Commbox
        {
            get { return commbox; }
        }

        protected Parameter Parameter
        {
            get { return param; }
        }

        public void StartHeartbeat(params byte[] bs)
        {
            StartHeartbeat(bs, 0, bs.Length);
        }

        public abstract void StartHeartbeat(byte[] buff, int offset, int count);

        public abstract void StopHeartbeat();

        public abstract int SendAndRecv(byte[] sData, int sOffset, int sCount, byte[] output);

        public abstract void StartCommunicate();

        public Timer ByteTxInterval
        {
            get { return byteTxInterval; }
            set { byteTxInterval = value; }
        }

        public Timer FrameTxInterval
        {
            get { return frameTxInterval; }
            set { frameTxInterval = value; }
        }

        public Timer ByteRxTimeout
        {
            get { return byteRxTimeout; }
            set { byteRxTimeout = value; }
        }

        public Timer FrameRxTimeout
        {
            get { return frameRxTimeout; }
            set { frameRxTimeout = value; }
        }

        public Timer HeartbeatInterval
        {
            get { return heartbeatInterval; }
            set { heartbeatInterval = value; }
        }

        protected void LeftShiftBuff(byte[] buff, int shiftSize, int length)
        {
            Array.Copy(buff, shiftSize, buff, 0, length);
        }
    }
}


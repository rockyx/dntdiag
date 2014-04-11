using System;
using System.Threading;
using DNT.Diag.Commbox;
using DNT.Diag.Commbox.W80;
using DNT.Diag.Attributes;

namespace DNT.Diag.Channel.W80
{
    internal class MikuniECU300Channel : Channel
    {
        public MikuniECU300Channel(Parameter param, W80Commbox commbox)
			: base(param, commbox)
        {
        }

        public override void StartHeartbeat(byte[] data, int offset, int count)
        {
            try
            {
                Commbox.BuffId = Utils.LoByte(W80Commbox.LINKBLOCK);
                Commbox.NewBatch();
                Commbox.SendOutData(data, offset, count);
                Commbox.EndBatch();
                Commbox.KeepLink(true);
            }
            catch (CommboxException)
            {
                throw new ChannelException("Mikuni ECU300 Start Heartbeat fail!");
            }
        }

        public override void StopHeartbeat()
        {
            try
            {
                Commbox.KeepLink(false);
            }
            catch (CommboxException)
            {
                throw new ChannelException("Mikuni ECU300 Stop heartbeat fail!");
            }
        }

        public override int SendAndRecv(byte[] sData, int sOffset, int sCount, byte[] output)
        {
            try
            {
                Commbox.BuffId = 0;
                Commbox.NewBatch();
                Commbox.SendOutData(sData, sOffset, sCount);
                Commbox.RunReceive(W80Commbox.RECEIVE);
                Commbox.EndBatch();
                Commbox.RunBatch(false);

                if (Commbox.ReadBytes(output, 0, 2) != 2)
                {
                    throw new ChannelException("Mikuni ECU300 read header fail!");
                }

                int length = ((output[0] & 0xFF) << 8) | (output[1] & 0xFF) - 1; // Skip
                // checksum

                if (Commbox.ReadBytes(output, 2, length) != length)
                {
                    throw new ChannelException("Mikuni ECU300 read data fail!");
                }

                if (Commbox.ReadBytes(output, length + 2, 1) != 1)
                {
                    throw new ChannelException("Mikuni ECU300 read checksum fail!");
                }

                Commbox.StopNow(false);
                Commbox.CheckResult(Timer.FromMilliseconds(500));
                Commbox.DelBatch();

                LeftShiftBuff(output, 2, length);
                return length;

            }
            catch (CommboxException)
            {
                try
                {
                    Commbox.DelBatch();
                }
                catch (CommboxException)
                {
                }

                throw new ChannelException("Mikuni ECU300 send and receive fail!");
            }
        }

        public override void StartCommunicate()
        {
            try
            {
                int cmd1 = 0;
                int cmd2 = W80Commbox.SET_NULL;
                int cmd3 = W80Commbox.SET_NULL;

                cmd1 = W80Commbox.RS_232 | W80Commbox.BIT9_MARK | W80Commbox.SEL_SL | W80Commbox.UN_DB20;
                cmd2 = 0xFF;
                cmd3 = 0x02;
                // cmd1 = W80Commbox.RS_232 | W80Commbox.BIT9_EVEN | W80Commbox.SEL_SL
                // | W80Commbox.UN_DB20;
                // cmd2 = 0xFF;
                // cmd3 = 0x03;

                Commbox.SetCommCtrl(W80Commbox.PWC | W80Commbox.RZFC | W80Commbox.CK, W80Commbox.SET_NULL);
                Commbox.SetCommLine(W80Commbox.SK_NO, W80Commbox.RK1);
                Commbox.SetCommLink(cmd1, cmd2, cmd3);
                Commbox.SetCommBaud(Parameter.KLineBaudRate);
                Commbox.SetCommTime(W80Commbox.SETBYTETIME, ByteTxInterval);
                Commbox.SetCommTime(W80Commbox.SETWAITTIME, FrameTxInterval);
                Commbox.SetCommTime(W80Commbox.SETRECBBOUT, ByteRxTimeout);
                Commbox.SetCommTime(W80Commbox.SETRECFROUT, FrameRxTimeout);
                Commbox.SetCommTime(W80Commbox.SETLINKTIME, HeartbeatInterval);

                Thread.Sleep(1000);
            }
            catch (CommboxException)
            {
                throw new ChannelException("Mikuni ECU300 Start communication fail!");
            }
        }
    }
}


using System;
using System.Threading;
using DNT.Diag.Commbox;
using DNT.Diag.Commbox.W80;
using DNT.Diag.Attributes;

namespace DNT.Diag.Channel.W80
{
    internal class ISO9141Channel : Channel
    {
        private int kline;
        private int lline;

        public ISO9141Channel(Parameter param, W80Commbox commbox)
			: base(param, commbox)
        {
            lline = W80Commbox.SK_NO;
            kline = W80Commbox.RK_NO;
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
            catch (CommboxException e)
            {
                throw new ChannelException(e.Message);
            }
        }

        public override void StopHeartbeat()
        {
            try
            {
                Commbox.KeepLink(false);
            }
            catch (CommboxException e)
            {
                throw new ChannelException(e.Message);
            }
        }

        public override void StartCommunicate()
        {
            try
            {
                if (!ConfigLines())
                {
                    throw new ChannelException("ISO9141 communication line wrong!");
                }

                Commbox.SetCommCtrl(W80Commbox.PWC | W80Commbox.RZFC | W80Commbox.CK, W80Commbox.SET_NULL);
                Commbox.SetCommLine(lline, kline);
                Commbox.SetCommLink(W80Commbox.RS_232 | W80Commbox.BIT9_MARK | W80Commbox.SEL_SL | W80Commbox.SET_DB20, W80Commbox.SET_NULL, W80Commbox.INVERTBYTE);
                Commbox.SetCommBaud(5);
                Commbox.SetCommTime(W80Commbox.SETBYTETIME, Timer.FromMilliseconds(5));
                Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(25));
                Commbox.SetCommTime(W80Commbox.SETRECBBOUT, Timer.FromMilliseconds(400));
                Commbox.SetCommTime(W80Commbox.SETRECFROUT, Timer.FromMilliseconds(500));
                Commbox.SetCommTime(W80Commbox.SETLINKTIME, Timer.FromMilliseconds(500));

                Thread.Sleep(1000);

                Commbox.BuffId = 0;
                Commbox.NewBatch();
                Commbox.SendOutData(Utils.LoByte(Parameter.KLineAddrCode));
                Commbox.SetCommLine(kline == W80Commbox.RK_NO ? lline : W80Commbox.SK_NO, kline);
                Commbox.RunReceive(W80Commbox.SET55_BAUD);
                Commbox.RunReceive(W80Commbox.REC_LEN_1);
                Commbox.TurnOverOneByOne();
                Commbox.RunReceive(W80Commbox.REC_LEN_1);
                Commbox.TurnOverOneByOne();
                Commbox.RunReceive(W80Commbox.REC_LEN_1);
                Commbox.EndBatch();

                int tempLength = 0;
                byte[] tempBuff = new byte[3];

                Commbox.RunBatch(false);
                tempLength = Commbox.ReadData(tempBuff, 0, 3,
                    Timer.FromMilliseconds(3));
                if (tempLength != 3)
                    throw new ChannelException("ISO9141 start communication read timeout");

                Commbox.CheckResult(Timer.FromMilliseconds(500));
                Commbox.DelBatch();

                Commbox.SetCommTime(W80Commbox.SETBYTETIME, Timer.FromMilliseconds(5));
                Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(15));
                Commbox.SetCommTime(W80Commbox.SETRECBBOUT, Timer.FromMilliseconds(80));
                Commbox.SetCommTime(W80Commbox.SETRECFROUT, Timer.FromMilliseconds(200));
            }
            catch (CommboxException e)
            {
                try
                {
                    Commbox.DelBatch();
                }
                catch
                {
                }
                throw new ChannelException(e.Message);
            }
        }

        private bool ConfigLines()
        {
            if (Parameter.KLineComLine == 7)
            {
                lline = W80Commbox.SK1;
                kline = W80Commbox.RK1;
            }
            else
            {
                return false;
            }

            return true;
        }

        private int singleUnpack(byte[] buff, int offset, int length)
        {
            length--; // data length.
            int checksum = 0;
            for (int i = 0; i < length; i++)
            {
                checksum += buff[offset + i];
                if (checksum != buff[length])
                {
                    throw new ChannelException(
                        "ISO9141 recv data but checksum error!");
                }
            }

            length -= 3;

            LeftShiftBuff(buff, offset + 3, length);
            return length;
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

                int pos = 0;
                int j = 3;
                int k = 0;
                int retlen = 0;
                while (true)
                {
                    if (Commbox.ReadBytes(output, pos++, 1) != 1)
                        break;
                }

                Commbox.StopNow(false);
                Commbox.CheckResult(Timer.FromMilliseconds(500));
                Commbox.DelBatch();

                if (pos < 5)
                    throw new ChannelException("ISO9141 receive data fail!");

                while (j < pos)
                {
                    // Multiple Frame
                    if (output[k] == output[j] && (output[k + 1] == output[j + 1])
                    && (output[k + 2] == output[j + 2]))
                    {
                        retlen += singleUnpack(output, k, k - j);
                        k = j;
                    }
                    j++;
                }

                // Add last frame or it's a single frame
                retlen += singleUnpack(output, k, j - k);

                return retlen;
            }
            catch (CommboxException e)
            {
                throw new ChannelException(e.Message);
            }
        }
    }
}


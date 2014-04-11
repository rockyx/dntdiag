using System;
using System.Threading;
using DNT.Diag.Commbox;
using DNT.Diag.Commbox.W80;
using DNT.Diag.Attributes;

namespace DNT.Diag.Channel.W80
{
    internal class MikuniECU200Channel : Channel
    {
        public MikuniECU200Channel(Attributes.Parameter attr, W80Commbox commbox)
			: base(attr, commbox)
        {
        }

        public override void StartHeartbeat(byte[] data, int offset, int count)
        {
            try
            {
                Commbox.BuffId = W80Commbox.LINKBLOCK;
                Commbox.NewBatch();
                Commbox.SendOutData(data, offset, count);
                Commbox.EndBatch();
                Commbox.KeepLink(true);
            }
            catch (CommboxException)
            {
                throw new ChannelException("Mikuni ECU 200 start heartbeat fail!");
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
                throw new ChannelException("Mikuni ECU200 stop heartbeat fail!");
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

                int pos = 0;
                byte before = 0;

                // Read header 0x48
                while (true)
                {
                    if (Commbox.ReadBytes(output, 0, 1) == 1)
                    {
                        if (output[0] == 0x48)
                        {
                            pos++;
                            break;
                        }
                    }
                    else
                    {
                        throw new ChannelException(
                            "Mikuni ECU200 read header fail!");
                    }
                }

                while (true)
                {
                    if (Commbox.ReadBytes(output, pos, 1) == 1)
                    {
                        if (before == 0x0D && output[pos] == 0x0A)
                        {
                            pos++;
                            break;
                        }
                        before = output[pos];
                        pos++;
                    }
                    else
                    {
                        throw new ChannelException("Mikuni ECU200 read data fail!");
                    }
                }

                Commbox.StopNow(false);
                Commbox.CheckResult(Timer.FromMilliseconds(500));
                Commbox.DelBatch();

                pos -= 3;
                LeftShiftBuff(output, 1, pos);

                return pos;

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
                throw new ChannelException("Mikuni ECU200 send and receive fail!");
            }
        }

        public override void StartCommunicate()
        {

            try
            {
                int cmd1 = 0;
                int cmd2 = W80Commbox.SET_NULL;
                int cmd3 = W80Commbox.SET_NULL;

                if (Parameter.KLineParity == KLineParity.None)
                {
                    cmd1 = W80Commbox.RS_232 | W80Commbox.BIT9_MARK | W80Commbox.SEL_SL | W80Commbox.UN_DB20;
                    cmd2 = 0xFF;
                    cmd3 = 0x02;
                }
                else
                {
                    cmd1 = W80Commbox.RS_232 | W80Commbox.BIT9_EVEN | W80Commbox.SEL_SL | W80Commbox.UN_DB20;
                    cmd2 = 0xFF;
                    cmd3 = 0x03;
                }

                Commbox.SetCommCtrl(W80Commbox.PWC | W80Commbox.RZFC | W80Commbox.CK | W80Commbox.REFC, W80Commbox.SET_NULL);
                Commbox.SetCommLine(W80Commbox.SK_NO, W80Commbox.RK1);
                Commbox.SetCommLink(cmd1, cmd2, cmd3);
                Commbox.SetCommBaud(Parameter.KLineBaudRate);
                Commbox.SetCommTime(W80Commbox.SETBYTETIME, Timer.FromMicroseconds(100));
                Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMicroseconds(1000));
                Commbox.SetCommTime(W80Commbox.SETRECBBOUT, Timer.FromMicroseconds(400000));
                Commbox.SetCommTime(W80Commbox.SETRECFROUT, Timer.FromMicroseconds(500000));
                Commbox.SetCommTime(W80Commbox.SETLINKTIME, Timer.FromMicroseconds(500000));

                Thread.Sleep(1000);
            }
            catch (CommboxException)
            {
                throw new ChannelException("Mikuni ECU200 start communication fail!");
            }
        }
    }
}


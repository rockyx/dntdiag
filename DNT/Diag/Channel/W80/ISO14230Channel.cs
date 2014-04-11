using System;
using System.Collections.Generic;
using System.Threading;
using DNT.Diag.Attributes;
using DNT.Diag.Commbox;
using DNT.Diag.Commbox.W80;

namespace DNT.Diag.Channel.W80
{
    internal class ISO14230Channel : Channel
    {
        private int kLine;
        private int lLine;
        private Dictionary<KWP2KStartType, Action> startComms;

        private void StartCommunicationInit()
        {
            startComms = new Dictionary<KWP2KStartType, Action>();
            startComms[KWP2KStartType.Fast] = () =>
            {
                try
                {
                    int valueOpen = 0;
                    if (Parameter.KLineL)
                        valueOpen = W80Commbox.PWC | W80Commbox.RZFC | W80Commbox.CK;
                    else
                        valueOpen = W80Commbox.PWC | W80Commbox.RZFC | W80Commbox.CK;

                    Commbox.BuffId = 0xFF;
                    Commbox.SetCommCtrl(valueOpen, W80Commbox.SET_NULL);
                    Commbox.SetCommLine(kLine, lLine);
                    Commbox.SetCommLink(W80Commbox.RS_232 | W80Commbox.BIT9_MARK | W80Commbox.SEL_SL | W80Commbox.UN_DB20,
                        W80Commbox.SET_NULL, 
                        W80Commbox.SET_NULL);
                    Commbox.SetCommBaud(Parameter.KLineBaudRate);
                    Commbox.SetCommTime(W80Commbox.SETBYTETIME, Timer.FromMilliseconds(5));
                    Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(0));
                    Commbox.SetCommTime(W80Commbox.SETRECBBOUT, Timer.FromMilliseconds(400));
                    Commbox.SetCommTime(W80Commbox.SETRECFROUT, Timer.FromMilliseconds(500));
                    Commbox.SetCommTime(W80Commbox.SETLINKTIME, Timer.FromMilliseconds(500));

                    Thread.Sleep(1000);

                    Commbox.BuffId = 0;
                    Commbox.NewBatch();
                    Commbox.SetLineLevel(W80Commbox.COMS, W80Commbox.SET_NULL);
                    Commbox.CommboxDelay(Timer.FromMilliseconds(25));
                    Commbox.SetLineLevel(W80Commbox.SET_NULL, W80Commbox.COMS);
                    Commbox.CommboxDelay(Timer.FromMilliseconds(25));
                    Commbox.SendOutData(Parameter.KWP2kFastCmd);
                    Commbox.RunReceive(W80Commbox.REC_FR);
                    Commbox.EndBatch();
                    Commbox.RunBatch(false);

                    byte[] rData = new byte[100];
                    ReadOneFrame(rData);

                    Commbox.CheckResult(Timer.FromMilliseconds(500));
                    Commbox.DelBatch();
                    Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(55));

                }
                catch
                {
                    try
                    {
                        Commbox.DelBatch();
                    }
                    catch
                    {
                    }

                    throw new ChannelException("ISO14230 start communication fail!");
                }
            };

            startComms[KWP2KStartType.Addr] = () =>
            {
                try
                {
                    Commbox.SetCommCtrl(W80Commbox.PWC | W80Commbox.REFC | W80Commbox.RZFC | W80Commbox.CK, W80Commbox.SET_NULL);
                    Commbox.SetCommBaud(5);
                    Commbox.SetCommTime(W80Commbox.SETBYTETIME, Timer.FromMilliseconds(5));
                    Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(12));
                    Commbox.SetCommTime(W80Commbox.SETRECBBOUT, Timer.FromMilliseconds(400));
                    Commbox.SetCommTime(W80Commbox.SETRECFROUT, Timer.FromMilliseconds(500));
                    Commbox.SetCommTime(W80Commbox.SETLINKTIME, Timer.FromMilliseconds(500));

                    Thread.Sleep(1000);

                    Commbox.BuffId = 0;

                    Commbox.NewBatch();
                    Commbox.SendOutData(Utils.LoByte(Parameter.KLineAddrCode));
                    Commbox.SetCommLine((kLine == W80Commbox.RK_NO) ? lLine : W80Commbox.SK_NO, kLine);
                    Commbox.RunReceive(W80Commbox.SET55_BAUD);
                    Commbox.RunReceive(W80Commbox.REC_LEN_1);
                    Commbox.TurnOverOneByOne();
                    Commbox.RunReceive(W80Commbox.REC_LEN_1);
                    Commbox.TurnOverOneByOne();
                    Commbox.RunReceive(W80Commbox.REC_LEN_1);
                    Commbox.EndBatch();
                    Commbox.RunBatch(false);

                    byte[] temp = new byte[3];
                    Commbox.ReadData(temp, 0, temp.Length, Timer.FromSeconds(5));

                    Commbox.CheckResult(Timer.FromSeconds(5));
                    Commbox.DelBatch();
                    Commbox.SetCommTime(W80Commbox.SETWAITTIME, Timer.FromMilliseconds(55));

                    if (temp[2] != 0)
                        throw new ChannelException(
                            "ISO14230 Addr code data in offset 2 not zero!");
                }
                catch (CommboxException)
                {
                    throw new ChannelException("ISO14230 Addr init fail!");
                }
            };
        }

        private int ReadMode80(byte[] buff)
        {
            int pos = 3;

            if (Commbox.ReadBytes(buff, pos++, 1) != 1)
            {
                throw new ChannelException("ISO14230 Read Mode80 Length Fail!");
            }

            int length = (buff[3] & 0xFF);

            if (Commbox.ReadBytes(buff, pos, length) != length)
            {
                throw new ChannelException("ISO14230 Read Mode80 data fail!");
            }

            pos += length;

            if (Commbox.ReadBytes(buff, pos, 1) != 1)
            {
                throw new ChannelException("ISO14230 Read Mode80 checksum fail!");
            }

            LeftShiftBuff(buff, 4, length);
            return length;
        }

        private int ReadMode8XCX(byte[] buff)
        {
            int length = buff[0] & 0xFF;
            length = (length & 0xC0) == 0xC0 ? length - 0xC0 : length - 0x80;

            int pos = 3;

            if (Commbox.ReadBytes(buff, pos, length) != length)
            {
                throw new ChannelException("ISO14230 Read ModeCX/8X Data Fail!");
            }

            pos += length;

            if (Commbox.ReadBytes(buff, pos, 1) != 1)
            {
                throw new ChannelException("ISO14230 Read ModeCX/8X Checksum Fail!");
            }

            LeftShiftBuff(buff, 3, length);
            return length;
        }

        private int ReadMode00(byte[] buff)
        {
            int length = buff[1] & 0xFF;

            int pos = 3;

            if (Commbox.ReadBytes(buff, pos, length) != length)
            {
                throw new ChannelException("ISO14230 Mode00 fail!");
            }

            LeftShiftBuff(buff, 2, length);
            return length;
        }

        private int ReadModeXX(byte[] buff)
        {
            int length = (buff[0] & 0xFF);

            if (Commbox.ReadBytes(buff, 3, length - 1) != (length - 1))
            {
                throw new ChannelException("ISO14230 Read ModeXX Fail!");
            }

            LeftShiftBuff(buff, 1, length);
            return length;
        }

        private int ReadOneFrame(byte[] output)
        {
            int len = Commbox.ReadBytes(output, 0, 3);

            if (len != 3)
                throw new ChannelException("ISO14230 Read Header Fail!");

            int temp0 = output[0] & 0xFF;
            int temp1 = output[1] & 0xFF;
            int length = 0;

            if (temp1 == Parameter.KLineSourceAddress)
            {
                if (temp0 == 0x80)
                {
                    length = ReadMode80(output);
                }
                else
                {
                    length = ReadMode8XCX(output);
                }
            }
            else
            {
                if (temp0 == 0x00)
                {
                    length = ReadMode00(output);
                }
                else
                {
                    length = ReadModeXX(output);
                }
            }

            return length;
        }

        private bool ConfigLines()
        {
            if (Parameter.KLineComLine == 7)
            {
                lLine = W80Commbox.SK1;
                kLine = W80Commbox.RK1;
            }
            else
            {
                return false;
            }

            return true;
        }

        public ISO14230Channel(Parameter param, W80Commbox commbox)
			: base(param, commbox)
        {
            kLine = W80Commbox.SK_NO;
            lLine = W80Commbox.RK_NO;
            StartCommunicationInit();
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
                throw new ChannelException("ISO14230 Start Heartbeat fail!");
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
                throw new ChannelException("ISO14230 Stop heartbeat fail!");
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

                int length = ReadOneFrame(output);

                Commbox.StopNow(false);
                Commbox.EndBatch();

                return length;
            }
            catch (CommboxException)
            {
                try
                {
                    Commbox.StopNow(false);
                    Commbox.DelBatch();
                }
                catch (CommboxException)
                {
                }

                throw new ChannelException("ISO14230 Send And Recv Fail!");
            }
        }

        public override void StartCommunicate()
        {
            if (!ConfigLines())
                throw new ChannelException("ISO14230 unsupported com line!");
            startComms[Parameter.KWP2KStartType]();
        }
    }
}


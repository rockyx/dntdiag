using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DNT.Diag.Data;
using DNT.Diag.Channel;

namespace DNT.Diag.ECU.Mikuni
{
    internal class PowertrainTroubleCodeECU200 : TroubleCodeFunction
    {
        private byte[] syntheticFailure;
        private Dictionary<int, byte[]> failureCmds;
        private Dictionary<int, Func<byte[], int, int, string>> failureCalcs;
        private byte[] failureHistoryPointer;
        private Dictionary<int, byte[]> failureHistoryBuffer;
        private byte[] failureHistoryClear;
        private static Dictionary<string, string> mikuni16CTroubleCodes;
        private static Dictionary<string, string> mikuni16ATroubleCodes;
        private PowertrainModel model;
        private string sys;
        private byte[] rData;

        static PowertrainTroubleCodeECU200()
        {
            mikuni16ATroubleCodes = new Dictionary<String, String>();
            mikuni16ATroubleCodes.Add("0040", "00");
            mikuni16ATroubleCodes.Add("0080", "00");
            mikuni16ATroubleCodes.Add("0140", "01");
            mikuni16ATroubleCodes.Add("0180", "01");
            mikuni16ATroubleCodes.Add("0240", "02");
            mikuni16ATroubleCodes.Add("0280", "02");
            mikuni16ATroubleCodes.Add("0340", "03");
            mikuni16ATroubleCodes.Add("0380", "03");
            mikuni16ATroubleCodes.Add("0540", "05");
            mikuni16ATroubleCodes.Add("0580", "05");
            mikuni16ATroubleCodes.Add("0640", "06");
            mikuni16ATroubleCodes.Add("0680", "06");
            mikuni16ATroubleCodes.Add("0740", "07");
            mikuni16ATroubleCodes.Add("0780", "07");
            mikuni16ATroubleCodes.Add("0840", "08");
            mikuni16ATroubleCodes.Add("0880", "08");
            mikuni16ATroubleCodes.Add("0940", "09");
            mikuni16ATroubleCodes.Add("0980", "09");
            mikuni16ATroubleCodes.Add("2040", "20");
            mikuni16ATroubleCodes.Add("2080", "20");
            mikuni16ATroubleCodes.Add("2140", "21");
            mikuni16ATroubleCodes.Add("2180", "21");
            mikuni16ATroubleCodes.Add("2240", "22");
            mikuni16ATroubleCodes.Add("2280", "22");
            mikuni16ATroubleCodes.Add("2340", "23");
            mikuni16ATroubleCodes.Add("2380", "23");
            mikuni16ATroubleCodes.Add("2440", "24");
            mikuni16ATroubleCodes.Add("2480", "24");
            mikuni16ATroubleCodes.Add("4040", "40");
            mikuni16ATroubleCodes.Add("4080", "40");

            mikuni16CTroubleCodes = new Dictionary<String, String>();
            mikuni16CTroubleCodes.Add("0040", "13");
            mikuni16CTroubleCodes.Add("0080", "13");
            mikuni16CTroubleCodes.Add("0140", "44");
            mikuni16CTroubleCodes.Add("0180", "44");
            mikuni16CTroubleCodes.Add("0240", "14");
            mikuni16CTroubleCodes.Add("0280", "14");
            mikuni16CTroubleCodes.Add("0340", "98");
            mikuni16CTroubleCodes.Add("0380", "98");
            mikuni16CTroubleCodes.Add("0540", "99");
            mikuni16CTroubleCodes.Add("0580", "99");
            mikuni16CTroubleCodes.Add("0640", "15");
            mikuni16CTroubleCodes.Add("0680", "15");
            mikuni16CTroubleCodes.Add("0740", "21");
            mikuni16CTroubleCodes.Add("0780", "21");
            mikuni16CTroubleCodes.Add("0840", "23");
            mikuni16CTroubleCodes.Add("0880", "23");
            mikuni16CTroubleCodes.Add("0940", "12");
            mikuni16CTroubleCodes.Add("0980", "12");
            mikuni16CTroubleCodes.Add("2040", "32");
            mikuni16CTroubleCodes.Add("2080", "32");
            mikuni16CTroubleCodes.Add("2140", "24");
            mikuni16CTroubleCodes.Add("2180", "24");
            mikuni16CTroubleCodes.Add("2240", "67");
            mikuni16CTroubleCodes.Add("2280", "67");
            mikuni16CTroubleCodes.Add("2340", "66");
            mikuni16CTroubleCodes.Add("2380", "66");
            mikuni16CTroubleCodes.Add("2440", "49");
            mikuni16CTroubleCodes.Add("2480", "49");
        }

        public PowertrainTroubleCodeECU200(PowertrainECU200 ecu)
			: base(ecu.Database, ecu.Channel, ecu.Format)
        {
            syntheticFailure = Format.Pack(Database.QueryCommand("Synthetic Failure", "Mikuni ECU200"));

            failureCmds = new Dictionary<int, byte[]>();
            failureCalcs = new Dictionary<int, Func<byte[], int, int, string>>();

            failureCmds.Add(1, Format.Pack(Database.QueryCommand("Manifold Pressure Failure", "Mikuni ECU200")));
            failureCalcs.Add(1, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0040", "0080");
            });

            failureCmds.Add(2, Format.Pack(Database.QueryCommand("O2 Sensor Failure", "Mikuni ECU200")));
            failureCalcs.Add(2, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0140", "0180");
            });

            failureCmds.Add(3, Format.Pack(Database.QueryCommand("TPS Sensor Failure", "Mikuni ECU200")));
            failureCalcs.Add(3, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0240", "0280");
            });

            failureCmds.Add(4, Format.Pack(Database.QueryCommand("Sensor Source Failure", "Mikuni ECU200")));
            failureCalcs.Add(4, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0340", "0380");
            });

            failureCmds.Add(5, Format.Pack(Database.QueryCommand("Battery Voltage Failure", "Mikuni ECU200")));
            failureCalcs.Add(5, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0540", "0580");
            });

            failureCmds.Add(6, Format.Pack(Database.QueryCommand("Engine Temperature Sensor Failure", "Mikuni ECU200")));
            failureCalcs.Add(6, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0640", "0680");
            });

            failureCmds.Add(7, Format.Pack(Database.QueryCommand("Manifold Temperature Failure", "Mikuni ECU200")));
            failureCalcs.Add(7, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0740", "0780");
            });

            failureCmds.Add(8, Format.Pack(Database.QueryCommand("Tilt Sensor Failure", "Mikuni ECU200")));
            failureCalcs.Add(8, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0840", "0880");
            });

            failureCmds.Add(9, Format.Pack(Database.QueryCommand("DCP Failure", "Mikuni ECU200")));
            failureCalcs.Add(9, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "2040", "2080");
            });

            failureCmds.Add(10, Format.Pack(Database.QueryCommand("Ignition Coil Failure", "Mikuni ECU200")));
            failureCalcs.Add(10, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "2140", "2180");
            });

            failureCmds.Add(11, Format.Pack(Database.QueryCommand("O2 Heater Failure", "Mikuni ECU200")));
            failureCalcs.Add(11, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "2240", "2280");
            });

            failureCmds.Add(11, Format.Pack(Database.QueryCommand("EEPROM Failure", "Mikuni ECU200")));
            failureCalcs.Add(11, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "4040", "4080");
            });

            failureCmds.Add(11, Format.Pack(Database.QueryCommand("Air Valve Failure", "Mikuni ECU200")));
            failureCalcs.Add(11, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "2340", "2380");
            });

            failureCmds.Add(11, Format.Pack(Database.QueryCommand("SAV Failure", "Mikuni ECU200")));
            failureCalcs.Add(11, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "2440", "2480");
            });

            failureCmds.Add(11, Format.Pack(Database.QueryCommand("CPS Failure", "Mikuni ECU200")));
            failureCalcs.Add(11, (buff, offset, length) =>
            {
                return CalcTroubleCode(buff, offset, length, "0940", "0980");
            });

            failureHistoryPointer = Format.Pack(Database.QueryCommand("Failure History Pointer", "Mikuni ECU200"));

            failureHistoryBuffer = new Dictionary<int, byte[]>();
            for (int i = 0; i < 16; i++)
            {
                failureHistoryBuffer.Add(i, 
                    Format.Pack(
                        Database.QueryCommand(
                            "Failure History Buffer" + Convert.ToInt32(i), "Mikuni ECU200")
                    )
                );
            }

            failureHistoryClear = Format.Pack(Database.QueryCommand("Failure History Clear", "Mikuni ECU200"));

            model = ecu.Model;
            switch (model)
            {
                case PowertrainModel.DCJ_16A:
                case PowertrainModel.DCJ_16C:
                case PowertrainModel.DCJ_10:
                    sys = "DCJ Mikuni ECU200";
                    break;
                case PowertrainModel.QM200GY_F:
                case PowertrainModel.QM200_3D:
                case PowertrainModel.QM200J_3L:
                    sys = "QingQi Mikuni ECU200";
                    break;
                default:
                    break;
            }

            rData = new byte[100];
        }

        private string CalcTroubleCode(byte[] recv, int offset, int count, string low, string high)
        {
            uint data = Convert.ToUInt32(Encoding.ASCII.GetString(recv, offset, count), 16);
            if ((data & 0x1C00) != 0)
                return low;
            else if ((data & 0xE000) != 0)
                return high;
            else
                return "0000";
        }

        private void PushTcs(List<TroubleCodeItem> tcs, string code)
        {
            TroubleCodeItem item = Database.QueryTroubleCode(code, sys);
            if (model == PowertrainModel.DCJ_16A)
                item.Code = mikuni16ATroubleCodes[code];
            else if (model == PowertrainModel.DCJ_16C)
                item.Code = mikuni16CTroubleCodes[code];
            tcs.Add(item);
        }

        public override List<DNT.Diag.Data.TroubleCodeItem> ReadCurrent()
        {
            try
            {
                List<TroubleCodeItem> tcs = new List<TroubleCodeItem>();
                int length = Channel.SendAndRecv(syntheticFailure, 0, syntheticFailure.Length, rData);

                if (rData[0] != '0' || rData[1] != '0' || rData[2] != '0' || rData[3] != '0')
                {
                    for (int i = 1; i <= 15; i++)
                    {
                        byte[] cmd = failureCmds[i];
                        length = Channel.SendAndRecv(cmd, 0, cmd.Length, rData);

                        if (rData[0] != '0' || rData[1] != '0' || rData[2] != '0' || rData[3] != '0')
                        {
                            string code = failureCalcs[i](rData, 0, rData.Length);
                            if (!code.Equals("0000"))
                                PushTcs(tcs, code);
                        }
                    }
                }

                return tcs;
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }

        public override List<TroubleCodeItem> ReadHistory()
        {
            try
            {
                List<TroubleCodeItem> tcs = new List<TroubleCodeItem>();

                int length = Channel.SendAndRecv(failureHistoryPointer, 0, failureHistoryPointer.Length, rData);

                int pointer = Convert.ToInt32(Encoding.ASCII.GetString(rData, 0, length), 16);

                for (int i = 0; i < 16; i++)
                {
                    int pos = pointer - i - 1;
                    if (pos < 0)
                        pos = pointer + 15 - i;

                    byte[] cmd = failureHistoryBuffer[pos];
                    length = Channel.SendAndRecv(cmd, 0, cmd.Length, rData);

                    if (rData[0] != '0' || rData[1] != '0' || rData[2] != '0' || rData[3] != '0')
                    {
                        string code = Encoding.ASCII.GetString(rData, 0, length);
                        if (!code.Equals("0000"))
                            PushTcs(tcs, code);
                    }
                }
                return tcs;
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }

        public override void Clear()
        {
            try
            {
                Channel.SendAndRecv(failureHistoryClear, 0, failureHistoryClear.Length, rData);

                if (rData[0] != 'A')
                {
                    throw new DiagException(Database.QueryText("Clear Trouble Code Fail", "System"));
                }

                Thread.Sleep(TimeSpan.FromSeconds(5));

                var tcs = ReadHistory();
                if (tcs != null || tcs.Count != 0)
                    throw new DiagException(Database.QueryText("Clear Trouble Code Fail", "System"));

            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }
    }
}


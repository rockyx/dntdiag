using System;
using System.Collections.Generic;
using DNT.Diag.Data;
using DNT.Diag.Channel;

namespace DNT.Diag.ECU.Mikuni
{
    internal class PowertrainTroubleCodeECU300 : TroubleCodeFunction
    {
        private byte[] syntheticFailure;
        private Dictionary<int, byte[]> failureCmds;
        private Dictionary<int, Func<byte[], int, int, string>> failureCalcs;
        private byte[] failureHistoryPointer;
        private Dictionary<int, byte[]> failureHistoryBuffer;
        private byte[] failureHistoryClear1;
        private byte[] failureHistoryClear2;
        private byte[] failureHistoryClear3;
        private PowertrainECU300 ecu;
        private PowertrainModel model;
        private string sys;
        private byte[] rData;

        public PowertrainTroubleCodeECU300(PowertrainECU300 ecu)
			: base(ecu.Database, ecu.Channel, ecu.Format)
        {
            syntheticFailure = Format.Pack(Database.QueryCommand("Synthetic Failure", "Mikuni ECU300"));

            failureCmds = new Dictionary<int, byte[]>();
            failureCalcs = new Dictionary<int, Func<byte[], int, int, string>>();

            byte[] cmd = Database.QueryCommand("O2 Sensor Failure", "Mikuni ECU300");
            failureCmds.Add(1, Format.Pack(cmd));
            failureCalcs.Add(1, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0140", "0180");
            });

            cmd = Database.QueryCommand("TPS Value Failure", "Mikuni ECU300");
            failureCmds.Add(2, Format.Pack(cmd));
            failureCalcs.Add(2, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0240", "0280");
            });

            cmd = Database.QueryCommand("Sensor Source Failure", "Mikuni ECU300");
            failureCmds.Add(3, Format.Pack(cmd));
            failureCalcs.Add(3, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0340", "0380");
            });

            cmd = Database.QueryCommand("Battery Voltage Failure", "Mikuni ECU300");
            failureCmds.Add(4, Format.Pack(cmd));
            failureCalcs.Add(4, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0540", "0580");
            });

            cmd = Database.QueryCommand("Engine Temperature Sensor Failure", "Mikuni ECU300");
            failureCmds.Add(5, Format.Pack(cmd));
            failureCalcs.Add(5, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0640", "0680");
            });

            cmd = Database.QueryCommand("Tilt Sensor Failure", "Mikuni ECU300");
            failureCmds.Add(6, Format.Pack(cmd));
            failureCalcs.Add(6, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "0880", "0880");
            });

            cmd = Database.QueryCommand("Injector Failure", "Mikuni ECU300");
            failureCmds.Add(7, Format.Pack(cmd));
            failureCalcs.Add(7, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "2040", "2080");
            });

            cmd = Database.QueryCommand("Ignition Coil Failure", "Mikuni ECU300");
            failureCmds.Add(8, Format.Pack(cmd));
            failureCalcs.Add(8, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "2140", "2180");
            });

            cmd = Database.QueryCommand("DSV Failure", "Mikuni ECU300");
            failureCmds.Add(9, Format.Pack(cmd));
            failureCalcs.Add(9, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "2840", "2880");
            });

            cmd = Database.QueryCommand("PDP Failure", "Mikuni ECU300");
            failureCmds.Add(10, Format.Pack(cmd));
            failureCalcs.Add(10, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "2740", "2780");
            });

            cmd = Database.QueryCommand("EEPROM Failure", "Mikuni ECU300");
            failureCmds.Add(11, Format.Pack(cmd));
            failureCalcs.Add(11, (recv, offset, count) =>
            {
                return CalcTroubleCode(recv, offset, count, "4040", "4080");
            });

            failureHistoryPointer = Format.Pack(Database.QueryCommand("Failure History Pointer", "Mikuni ECU300"));

            failureHistoryBuffer = new Dictionary<int, byte[]>();
            for (int i = 1; i < 17; i++)
            {
                cmd = Database.QueryCommand("Failure History Buffer" + Convert.ToString(i), "Mikuni ECU300");
                failureHistoryBuffer.Add(i, Format.Pack(cmd));
            }

            failureHistoryClear1 = Format.Pack(Database.QueryCommand("Failure History Clear1", "Mikuni ECU300"));

            failureHistoryClear2 = Format.Pack(Database.QueryCommand("Failure History Clear2", "Mikuni ECU300"));

            failureHistoryClear3 = Format.Pack(Database.QueryCommand("Failure History Clear3", "Mikuni ECU300"));

            this.ecu = ecu;
            this.model = ecu.Model;

            switch (model)
            {
                case PowertrainModel.QM48QT_8:
                    sys = "QingQi Mikuni ECU300";
                    break;
                default:
                    break;
            }

            rData = new byte[128];
        }

        private static string CalcTroubleCode(byte[] recv, int offset, int count, string low, string high)
        {
            int value = recv[offset + 1];
            if ((value & 0x04) != 0)
            {
                return high;
            }
            else if ((value & 0x20) != 0)
            {
                return low;
            }
            return null;
        }

        public override List<DNT.Diag.Data.TroubleCodeItem> ReadCurrent()
        {
            try
            {
                List<TroubleCodeItem> tcs = new List<TroubleCodeItem>();

                int length = Channel.SendAndRecv(syntheticFailure, 0, syntheticFailure.Length, rData);

                if (!PowertrainECU300.CheckIfPositive(rData, syntheticFailure))
                    throw new DiagException(Database.QueryText("Read Trouble Code Fail", "System"));

                if (rData[1] != 0x00 || rData[2] != 0x00)
                {
                    for (int i = 1; i <= 11; i++)
                    {
                        byte[] cmd = failureCmds[i];
                        length = Channel.SendAndRecv(cmd, 0, cmd.Length, rData);

                        if (!PowertrainECU300.CheckIfPositive(rData, cmd))
                        {
                            throw new DiagException(Database.QueryText("Read Trouble Code Fail", "System"));
                        }

                        if ((rData[1] != 0x00) || (rData[2] != 0x00))
                        {
                            string code = failureCalcs[i](rData, 1, length);
                            if (code != null)
                                tcs.Add(Database.QueryTroubleCode(code, sys));
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

                Channel.SendAndRecv(failureHistoryPointer, 0, failureHistoryPointer.Length, rData);

                if (!PowertrainECU300.CheckIfPositive(rData, failureHistoryPointer))
                {
                    throw new DiagException(Database.QueryText("Read Trouble Code Fail", "System"));
                }

                int pointer = rData[2];

                for (int i = 1; i < 17; i++)
                {
                    int pos = pointer + 1 - i;
                    if (pos <= 0)
                        pos = i;

                    byte[] cmd = failureHistoryBuffer[pos];
                    Channel.SendAndRecv(cmd, 0, cmd.Length, rData);

                    if (!PowertrainECU300.CheckIfPositive(rData, cmd))
                    {
                        throw new DiagException(Database.QueryText("Read Trouble Code Fail", "System"));
                    }

                    if (rData[1] != 0x00 || rData[2] != 0x00)
                    {
                        string code = String.Format("{0:X4}", rData[1] * 256 + rData[2]);
                        TroubleCodeItem item = Database.QueryTroubleCode(code, sys);
                        if (!tcs.Contains(item))
                        {
                            tcs.Add(item);
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

        public override void Clear()
        {
            try
            {
                PowertrainECU300.CheckEngineStop(ecu);
                Channel.SendAndRecv(failureHistoryClear1, 0, failureHistoryClear1.Length, rData);

                if (!PowertrainECU300.CheckIfPositive(rData, failureHistoryClear1))
                    throw new DiagException(Database.QueryText("Clear Trouble Code Fail", "System"));

                Channel.SendAndRecv(failureHistoryClear2, 0, failureHistoryClear2.Length, rData);

                if (!PowertrainECU300.CheckIfPositive(rData, failureHistoryClear2))
                    throw new DiagException(Database.QueryText("Clear Trouble Code Fail", "System"));

                Channel.SendAndRecv(failureHistoryClear3, 0, failureHistoryClear3.Length, rData);
                if (!PowertrainECU300.CheckIfPositive(rData, failureHistoryClear3))
                    throw new DiagException(Database.QueryText("Clear Trouble Code Fail", "System"));
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }
    }
}


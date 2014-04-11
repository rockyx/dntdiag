using System;
using System.Collections.Generic;

namespace DNT.Diag.ECU.Mikuni
{
    internal class PowertrainDataStreamECU300 : DataStreamFunction
    {
        private PowertrainModel model;

        public PowertrainDataStreamECU300(PowertrainECU300 ecu)
			: base(ecu.Database, ecu.Channel, ecu.Format)
        {
            model = ecu.Model;

            switch (model)
            {
                case PowertrainModel.QM48QT_8:
                    if (!QueryLiveData("QingQi Mikuni ECU300"))
                    {
                        throw new DiagException("Cannot find live datas");
                    }

                    foreach (var item in LiveDataItems)
                    {
                        item.FormattedCommand = Format.Pack(item.Command);
                        item.IsEnabled = true;
                    }

                    LiveDataItems["TS"].IsEnabled = false;
                    break;
                default:
                    throw new DiagException("Unsupport model");
            }

            ReadInterval = Timer.FromMilliseconds(10);
        }

        protected override void InitCalcDelegates()
        {
            HistoryBuff.Add("ER", new byte[2]);
            LiveDataItems["ER"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ER"];
                if ((buff[0] != item.EcuResponseBuff[1]) || (buff[1] != item.EcuResponseBuff[2]))
                {
                    buff[0] = item.EcuResponseBuff[1];
                    buff[1] = item.EcuResponseBuff[2];
                    int value = ((buff[0] * 256) + buff[1]) * 500 / 256;
                    item.Value = Convert.ToString(value);
                }
            };

            HistoryBuff.Add("BV", new byte[2]);
            LiveDataItems["BV"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["BV"];
                if ((buff[0] != item.EcuResponseBuff[1]) || (buff[1] != item.EcuResponseBuff[2]))
                {
                    buff[0] = item.EcuResponseBuff[1];
                    buff[1] = item.EcuResponseBuff[2];
                    double value = ((double)(buff[0] * 256 + buff[1])) * 18.75 / 65536;
                    item.Value = String.Format("{0:F1}", value);
                }
            };

            HistoryBuff.Add("TPS", new byte[2]);
            LiveDataItems["TPS"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["TPS"];
                if ((buff[0] != item.EcuResponseBuff[1]) || (buff[1] != item.EcuResponseBuff[2]))
                {
                    buff[0] = item.EcuResponseBuff[1];
                    buff[1] = item.EcuResponseBuff[2];
                    double value = ((double)(buff[0] * 256 + buff[1])) * 100 / 4096;
                    item.Value = String.Format("{0:F1}", value);
                }
            };

            HistoryBuff.Add("ET", new byte[2]);
            LiveDataItems["ET"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ET"];
                if ((buff[0] != item.EcuResponseBuff[1]) || (buff[1] != item.EcuResponseBuff[2]))
                {
                    buff[0] = item.EcuResponseBuff[1];
                    buff[1] = item.EcuResponseBuff[2];
                    double value = ((double)(buff[0] * 256 + buff[1])) / 256 - 50;
                    item.Value = String.Format("{0:F1}", value);
                }
            };

            HistoryBuff.Add("TS", new byte[1]);
            LiveDataItems["TS"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["TS"];
                if (buff[0] != item.EcuResponseBuff[1])
                {
                    buff[0] = item.EcuResponseBuff[1];
                    if (buff[0] != 0)
                    {
                        item.Value = Database.QueryText("Tilt", "System");
                    }
                    else
                    {
                        item.Value = Database.QueryText("No Tilt", "System");
                    }
                }
            };

            HistoryBuff.Add("ERF", new byte[1]);
            LiveDataItems["ERF"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ERF"];
                if (buff[0] != item.EcuResponseBuff[1])
                {
                    buff[0] = item.EcuResponseBuff[1];
                    if (buff[0] == 1)
                    {
                        item.Value = Database.QueryText("Running", "System");
                    }
                    else
                    {
                        item.Value = Database.QueryText("Stopped", "System");
                    }
                }
            };
        }
    }
}


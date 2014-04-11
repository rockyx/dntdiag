using System;
using System.Text;
using DNT.Diag.Data;

namespace DNT.Diag.ECU.Mikuni
{
    internal class PowertrainDataStreamECU200 : DataStreamFunction
    {
        private PowertrainModel model;

        public PowertrainDataStreamECU200(PowertrainECU200 ecu)
			: base(ecu.Database, ecu.Channel, ecu.Format)
        {
            model = ecu.Model;

            switch (model)
            {
                case PowertrainModel.DCJ_16A:
                case PowertrainModel.DCJ_16C:
                case PowertrainModel.DCJ_10:
                    if (!QueryLiveData("DCJ Mikuni ECU200"))
                    {
                        throw new DiagException("Cannot find live datas");
                    }

                    foreach (var item in LiveDataItems)
                    {
                        item.FormattedCommand = Format.Pack(item.Command);
                        item.IsEnabled = true;
                    }
                    break;
                case PowertrainModel.QM200GY_F:
                case PowertrainModel.QM200_3D:
                case PowertrainModel.QM200J_3L:
                    if (!QueryLiveData("QingQi Mikuni ECU200"))
                    {
                        throw new DiagException("Cannot find live datas");
                    }

                    foreach (var item in LiveDataItems)
                    {
                        item.FormattedCommand = Format.Pack(item.Command);
                        item.IsEnabled = true;
                    }
                    LiveDataItems["TS"].IsEnabled = false;
                    LiveDataItems["ERF"].IsEnabled = false;
                    LiveDataItems["IS"].IsEnabled = false;
                    break;

                default:
                    throw new DiagException("Unsupport model!");
            }

            ReadInterval = Timer.FromMilliseconds(10);
        }

        protected override void InitCalcDelegates()
        {
            HistoryBuff.Add("ER", new byte[10]);
            LiveDataItems["ER"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ER"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        ) * 500) / 256;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F0}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("BV", new byte[10]);
            LiveDataItems["BV"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["BV"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 512;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F0}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("TPS", new byte[10]);
            LiveDataItems["TPS"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["BV"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 512;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F1}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("MAT", new byte[10]);
            LiveDataItems["MAT"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["MAT"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 256 - 50;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F1}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("ET", new byte[10]);
            LiveDataItems["ET"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ET"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 256 - 50;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F1}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("BP", new byte[10]);
            LiveDataItems["BP"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["BP"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        item.Value = String.Format(
                            "{0:F1}", 
                            (Convert.ToDouble(
                                Convert.ToInt32(
                                    Encoding.ASCII.GetString(
                                        buff, 0, item.EcuResponseBuff.Length), 16
                                )
                            )) / 512);
                        return;
                    }
                }
            };

            HistoryBuff.Add("MP", new byte[10]);
            LiveDataItems["MP"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["MP"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 512;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F1}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("IT", new byte[10]);
            LiveDataItems["IT"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["IT"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        ) * 15) / 256 - 22.5;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F1}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("IPW", new byte[10]);
            LiveDataItems["IPW"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["IPW"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        double v = (Convert.ToDouble(
                            Convert.ToInt32(
                                Encoding.ASCII.GetString(
                                    buff, 0, item.EcuResponseBuff.Length), 16
                            )
                        )) / 2;
                        CheckOutOfRange(v, item);
                        item.Value = String.Format("{0:F0}", v);
                        return;
                    }
                }
            };

            HistoryBuff.Add("TS", new byte[1]);
            LiveDataItems["TS"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["TS"];
                if (buff[0] != item.EcuResponseBuff[0])
                {
                    buff[0] = item.EcuResponseBuff[0];
                    if ((buff[0] & 0x40) != 0)
                    {
                        item.Value = Database.QueryText("Tilt", "System");
                    }
                    else
                    {
                        item.Value = Database.QueryText("No Tilt", "System");
                    }
                }
            };

            HistoryBuff.Add("ERF", new byte[10]);
            LiveDataItems["ERF"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["ERF"];
                for (int i = 0; i < item.EcuResponseBuff.Length; i++)
                {
                    if (buff[i] != item.EcuResponseBuff[i])
                    {
                        Array.Copy(item.EcuResponseBuff.Buff, buff, item.EcuResponseBuff.Length);
                        if ((Convert.ToUInt16(
                            Encoding.ASCII.GetString(
                                buff, 0, item.EcuResponseBuff.Length), 16
                        ) & 0x0001) == 1)
                        {
                            item.Value = Database.QueryText("Running", "System");
                        }
                        else
                        {
                            item.Value = Database.QueryText("Stopped", "System");
                        }
                        return;
                    }
                }
            };

            HistoryBuff.Add("IS", new byte[1]);
            LiveDataItems["IS"].CalcDelegate = (item) =>
            {
                byte[] buff = HistoryBuff["IS"];
                if (buff[0] != item.EcuResponseBuff[0])
                {
                    buff[0] = item.EcuResponseBuff[0];
                    if ((buff[0] & 0x40) != 0)
                    {
                        item.Value = Database.QueryText("Idle", "System");
                    }
                    else
                    {
                        item.Value = Database.QueryText("Not Idle", "System");
                    }

                }
            };
        }
    }
}


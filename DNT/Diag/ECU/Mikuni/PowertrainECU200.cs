using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DNT.Diag.DB;
using DNT.Diag.Commbox;
using DNT.Diag.Channel;
using DNT.Diag.Attributes;
using DNT.Diag.Formats;

namespace DNT.Diag.ECU.Mikuni
{
    public class PowertrainECU200 : AbstractECU
    {
        private PowertrainModel model;
        private byte[] readECUVersion1;
        private byte[] readECUVersion2;
        private byte[] engineRevolutions;
        private byte[] tpsIdleAdjustments;
        private byte[] longTermLearnValueZoneInitialization;
        private Dictionary<int, byte[]> longTermLearnValueZones;
        private byte[] iscLearnValueInitialization;
        private byte[] rData;

        public PowertrainECU200(VehicleDB db, ICommbox box, PowertrainModel model)
			: base(db, box)
        {
            switch (model)
            {
                case PowertrainModel.DCJ_16A:
                case PowertrainModel.DCJ_16C:
                case PowertrainModel.DCJ_10:
                    Parameter.KLineParity = KLineParity.None;
                    Parameter.KLineBaudRate = 19200;
                    Channel = ChannelFactory.Create(Parameter, box, ProtocolType.MikuniECU200);
                    Format = new MikuniECU200Format(Parameter);
                    break;
                case PowertrainModel.QM200GY_F:
                case PowertrainModel.QM200_3D:
                case PowertrainModel.QM200J_3L:
                    Parameter.KLineParity = KLineParity.Even;
                    Parameter.KLineBaudRate = 19200;
                    Channel = ChannelFactory.Create(Parameter, box, ProtocolType.MikuniECU300);
                    Format = new MikuniECU200Format(Parameter);
                    break;
                default:
                    throw new DiagException("Unsupport model!");
            }
            this.model = model;
            if (Channel == null)
                throw new DiagException("Cannot create channel!!!");

            readECUVersion1 = Format.Pack(Database.QueryCommand("Read ECU Version 1", "Mikuni ECU200"));
            readECUVersion2 = Format.Pack(Database.QueryCommand("Read ECU Version 2", "Mikuni ECU200"));
            engineRevolutions = Format.Pack(Database.QueryCommand("Engine Revolutions", "Mikuni ECU200"));
            tpsIdleAdjustments = Format.Pack(Database.QueryCommand("TPS Idle Adjustment", "Mikuni ECU200"));
            longTermLearnValueZoneInitialization = Format.Pack(Database.QueryCommand("Long Term Learn Value Zone Initialization", "Mikuni ECU200"));
            longTermLearnValueZones = new Dictionary<int, byte[]>();
            for (int i = 1; i < 11; i++)
            {
                longTermLearnValueZones.Add(i, 
                    Format.Pack(
                        Database.QueryCommand(
                            "Long Term Learn Value Zone_" + Convert.ToString(i), "Mikuni ECU200")
                    )
                );
            }
            iscLearnValueInitialization = Format.Pack(Database.QueryCommand("ISC Learn Value Initialization", "Mikuni ECU200"));
            rData = new byte[100];

            DataStream = new PowertrainDataStreamECU200(this);
            TroubleCode = new PowertrainTroubleCodeECU200(this);
        }

        internal PowertrainModel Model
        {
            get { return model; }
        }

        private static PowertrainVersion FormatVersion(string hex)
        {
            PowertrainVersion ver = new PowertrainVersion();

            StringBuilder temp = new StringBuilder();
            temp.Append("ECU");

            for (int i = 0; i < 6; i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                    temp.Append(c);
            }

            temp.Append('-');

            int beginOfSoftware = 18;
            for (int i = 6; i < 16; i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                {
                    temp.Append(c);
                }
                else
                {
                    beginOfSoftware -= 2;
                }
            }

            ver.Hardware = temp.ToString();
            temp.Clear();

            for (int i = beginOfSoftware; i < (beginOfSoftware + 12); i += 2)
            {
                string e = hex.Substring(i, 2);
                byte h = Convert.ToByte(e, 16);
                char c = Convert.ToChar(h);
                if (Char.IsLetterOrDigit(c))
                    temp.Append(c);
            }

            ver.Software = temp.ToString();
            return ver;
        }

        public override void ChannelInit()
        {
            try
            {
                Channel.StartCommunicate();
            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }

        public PowertrainVersion ReadVersion()
        {
            try
            {
                int length = Channel.SendAndRecv(readECUVersion1, 0, readECUVersion1.Length, rData);

                Array.Copy(rData, 0, readECUVersion2, 3, 4);

                length = Channel.SendAndRecv(readECUVersion2, 0, readECUVersion2.Length, rData);

                string temp1 = Encoding.ASCII.GetString(rData, 0, length);
                PowertrainVersion ver = FormatVersion(temp1);

                switch (model)
                {
                    case PowertrainModel.DCJ_10:
                    case PowertrainModel.DCJ_16A:
                    case PowertrainModel.DCJ_16C:
                        break;
                    case PowertrainModel.QM200GY_F:
                        ver.Model = "M16-02";
                        break;
                    case PowertrainModel.QM200_3D:
                        ver.Model = "M16-01";
                        break;
                    case PowertrainModel.QM200J_3L:
                        ver.Model = "M16-03";
                        break;
                    default:
                        throw new DiagException("Unsupport model!");
                }

                return ver;
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }

        public void TPSIdleSetting()
        {
            try
            {
                Channel.SendAndRecv(engineRevolutions, 0, engineRevolutions.Length, rData);

                if (rData[0] != '0' || rData[1] != '0' || rData[2] != '0' || rData[3] != '0')
                {
                    throw new DiagException(Database.QueryText("Engine RPM Not Zero", "System"));
                }

                Channel.SendAndRecv(tpsIdleAdjustments, 0, tpsIdleAdjustments.Length, rData);

                if (rData[0] != 'A')
                    throw new DiagException(Database.QueryText("TPS Idle Setting Fail", "Mikuni"));
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }

        public void LongTermLearnValueZoneInitialization()
        {
            try
            {
                Channel.SendAndRecv(longTermLearnValueZoneInitialization, 0, longTermLearnValueZoneInitialization.Length, rData);

                if (rData[0] != 'A')
                    throw new DiagException(Database.QueryText("Long Term Learn Value Zone Initialization Fail", "System"));

                Thread.Sleep(TimeSpan.FromSeconds(5));

                for (int i = 1; i < 11; i++)
                {
                    byte[] cmd = longTermLearnValueZones[i];
                    Channel.SendAndRecv(cmd, 0, cmd.Length, rData);

                    if (rData[0] != '0' || rData[1] != '0' || rData[2] != '0' || rData[3] != '0')
                        throw new DiagException(Database.QueryText("Long Term Learn Value Zone Initialization Fail", "System"));
                }
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }

        public void ISCLearnValueInitialization()
        {
            try
            {
                Channel.SendAndRecv(iscLearnValueInitialization, 0, iscLearnValueInitialization.Length, rData);

                if (rData[0] != 'A')
                    throw new DiagException(Database.QueryText("ISC Learn Value Initialization Fail", "System"));
            }
            catch (ChannelException)
            {
                throw new DiagException(Database.QueryText("Communication Fail", "System"));
            }
        }
    }
}


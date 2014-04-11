using System;
using System.Text;
using DNT.Diag.DB;
using DNT.Diag.Commbox;
using DNT.Diag.Attributes;
using DNT.Diag.Channel;
using DNT.Diag.Formats;
using DNT.Diag.Data;

namespace DNT.Diag.ECU.Mikuni
{
    public class PowertrainECU300 : AbstractECU
    {
        private byte[] startConnection;
        private byte[] tpsIdleLearningValueSetting;
        private byte[] longTermLearningValueReset;
        private byte[] dsvISCLearningValueSetting;
        private byte[] readEcuVersion;
        private byte[] rData;
        private PowertrainModel model;

        public PowertrainECU300(VehicleDB db, ICommbox box, PowertrainModel model)
			: base(db, box)
        {
            switch (model)
            {
                case PowertrainModel.QM48QT_8:
                    Parameter.KLineParity = KLineParity.None;
                    Parameter.KLineBaudRate = 19200;
                    Channel = ChannelFactory.Create(Parameter, box, ProtocolType.MikuniECU300);
                    Format = new MikuniECU300Format(Parameter);
                    break;
                default:
                    throw new DiagException("Unsupport model!");
            }
            startConnection = Format.Pack(Database.QueryCommand("Start Connection", "Mikuni ECU300"));
            tpsIdleLearningValueSetting = Format.Pack(Database.QueryCommand("TPS Idle Learning Value Setting", "Mikuni ECU300"));
            longTermLearningValueReset = Format.Pack(Database.QueryCommand("02 Feed Back Long Term Learning Value Reset", "Mikuni ECU300"));
            dsvISCLearningValueSetting = Format.Pack(Database.QueryCommand("DSV ISC Learning Value Reset", "Mikuni ECU300"));
            readEcuVersion = Format.Pack(Database.QueryCommand("ECU Version Information", "Mikuni ECU300"));

            this.model = model;
            rData = new byte[128];

            DataStream = new PowertrainDataStreamECU300(this);
        }

        public static bool CheckIfPositive(byte[] rData, byte[] cmd)
        {
            if ((rData[0] & 0xFF) != (cmd[2] & 0xFF) + 0X40)
                return false;
            return true;
        }

        public PowertrainModel Model
        {
            get { return model; }
        }

        public override void ChannelInit()
        {
            try
            {
                Channel.ByteTxInterval = Timer.FromMicroseconds(100);
                Channel.FrameTxInterval = Timer.FromMicroseconds(1000);
                Channel.ByteRxTimeout = Timer.FromMicroseconds(400000);
                Channel.FrameRxTimeout = Timer.FromSeconds(2);
                Channel.HeartbeatInterval = Timer.FromMilliseconds(500);
                Channel.StartCommunicate();
                Channel.SendAndRecv(startConnection, 0, startConnection.Length, rData);
                if (rData[0] != 0x40)
                    throw new DiagException("Start Connection Fail!");
            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }

        public static void CheckEngineStop(PowertrainECU300 ecu)
        {
            try
            {
                var item = ecu.DataStream.LiveDataItems["ERF"];
                byte[] buff = item.EcuResponseBuff.Buff;
                byte[] cmd = item.FormattedCommand;
                ecu.Channel.SendAndRecv(cmd, 0, cmd.Length, buff);

                if (!CheckIfPositive(buff, cmd))
                {
                    throw new DiagException(ecu.Database.QueryText("Checking Engine Status Fail", "Mikuni"));
                }

                if (item.EcuResponseBuff[1] == 1)
                {
                    throw new DiagException(ecu.Database.QueryText("Function Fail Because ERF", "Mikuni"));
                }
            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }

        public void TPSIdleLearningValueSetting()
        {
            try
            {
                var tcs = TroubleCode.ReadCurrent();

                if (tcs != null || tcs.Count != 0)
                {
                    throw new DiagException(Database.QueryText("Function Fail Because TroubleCodes", "Mikuni"));
                }

                CheckEngineStop(this);

                Channel.SendAndRecv(tpsIdleLearningValueSetting, 0, tpsIdleLearningValueSetting.Length, rData);
                if (!CheckIfPositive(rData, tpsIdleLearningValueSetting))
                {
                    throw new DiagException(Database.QueryText("TPS Idle Setting Fail", "Mikuni"));
                }

            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }

        public void LongTermLearningValueReset()
        {
            try
            {
                CheckEngineStop(this);

                Channel.SendAndRecv(longTermLearningValueReset, 0, longTermLearningValueReset.Length, rData);

                if (!CheckIfPositive(rData, longTermLearningValueReset))
                {
                    throw new DiagException(Database.QueryText("Long Term Learn Value Zone Initialization Fail", "Mikuni"));
                }
            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }

        public void DSVISCLearnValueSetting()
        {
            try
            {
                CheckEngineStop(this);

                Channel.SendAndRecv(dsvISCLearningValueSetting, 0, dsvISCLearningValueSetting.Length, rData);
                if (!CheckIfPositive(rData, dsvISCLearningValueSetting))
                {
                    throw new DiagException(Database.QueryText("DSV ISC Learning Value Reset Fail", "Mikuni"));
                }
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
                int length = Channel.SendAndRecv(readEcuVersion, 0, readEcuVersion.Length, rData);

                if (!CheckIfPositive(rData, readEcuVersion))
                {
                    throw new DiagException(Database.QueryText("Read ECU Version Fail", "System"));
                }

                PowertrainVersion ver = new PowertrainVersion();
                ver.Model = "";

                ver.Hardware = "";
                ver.Software = Encoding.ASCII.GetString(rData, 1, length - 1);
                return ver;
            }
            catch (ChannelException e)
            {
                throw new DiagException(e.Message);
            }
        }
    }
}


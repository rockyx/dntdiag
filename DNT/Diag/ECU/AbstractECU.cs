using System;
using DNT.Diag.DB;
using DNT.Diag.Formats;
using DNT.Diag.Channel;
using DNT.Diag.Commbox;
using DNT.Diag.Attributes;

namespace DNT.Diag.ECU
{
    public abstract class AbstractECU
    {
        private TroubleCodeFunction troubleCode;
        private DataStreamFunction dataStream;
        private DataStreamFunction freezeFrame;
        private ActiveTestFunction activeTest;
        private VehicleDB db;
        private ICommbox box;
        private IFormat format;
        private Parameter param;
        private IChannel chn;

        public AbstractECU(VehicleDB db, ICommbox box)
        {
            troubleCode = null;
            dataStream = null;
            freezeFrame = null;
            activeTest = null;
            this.db = db;
            this.box = box;
            param = new Parameter();
            format = null;
            chn = null;
        }

        public TroubleCodeFunction TroubleCode
        {
            get { return troubleCode; }
            protected set { troubleCode = value; }
        }

        public DataStreamFunction DataStream
        {
            get { return dataStream; }
            protected set { dataStream = value; }
        }

        public DataStreamFunction FreezeFrame
        {
            get { return freezeFrame; }
            protected set { freezeFrame = value; }
        }

        public ActiveTestFunction ActiveTest
        {
            get { return activeTest; }
            set { activeTest = value; }
        }

        public ICommbox Commbox
        {
            get { return box; }
        }

        public IChannel Channel
        {
            get { return chn; }
            set { chn = value; }
        }

        public VehicleDB Database
        {
            get { return db; }
        }

        public IFormat Format
        {
            get { return format; }
            protected set { format = value; }
        }

        public Parameter Parameter
        {
            get { return param; }
        }

        public abstract void ChannelInit();
    }
}


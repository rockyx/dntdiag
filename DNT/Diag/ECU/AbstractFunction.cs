using System;
using DNT.Diag.DB;
using DNT.Diag.Channel;
using DNT.Diag.Formats;

namespace DNT.Diag.ECU
{
    public abstract class AbstractFunction
    {
        private VehicleDB db;
        private IChannel chn;
        private IFormat format;

        public AbstractFunction(VehicleDB db, IChannel chn, IFormat format)
        {
            this.db = db;
            this.chn = chn;
            this.format = format;
        }

        protected VehicleDB Database
        {
            get { return db; }
        }

        protected IChannel Channel
        {
            get { return chn; }
        }

        protected IFormat Format
        {
            get { return format; }
        }
    }
}


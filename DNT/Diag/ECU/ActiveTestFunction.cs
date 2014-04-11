using System;
using System.Collections.Generic;
using DNT.Diag.DB;
using DNT.Diag.Formats;
using DNT.Diag.Channel;

namespace DNT.Diag.ECU
{
    public abstract class ActiveTestFunction : AbstractFunction
    {
        private Dictionary<int, Action<ActiveState>> actMap;
        private ActiveState state;

        public ActiveTestFunction(VehicleDB db, IChannel chn, IFormat format)
			: base(db, chn, format)
        {
            actMap = new Dictionary<int, Action<ActiveState>>();
            state = ActiveState.Stop;
        }

        public abstract void Execute(int mode);

        public void ChangeState(ActiveState state)
        {
            lock (this)
            {
                this.state = state;
            }
        }

        protected Dictionary<int, Action<ActiveState>> ActMap
        {
            get
            {
                lock (this)
                    return actMap;
            }
        }

        protected ActiveState State
        {
            get
            {
                lock (this)
                    return state;
            }
        }
    }
}


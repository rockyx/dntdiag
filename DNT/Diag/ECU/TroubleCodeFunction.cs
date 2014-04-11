using System;
using System.Text;
using System.Collections.Generic;
using DNT.Diag.DB;
using DNT.Diag.Channel;
using DNT.Diag.Data;
using DNT.Diag.Formats;

namespace DNT.Diag.ECU
{
    public abstract class TroubleCodeFunction : AbstractFunction
    {
        public TroubleCodeFunction(VehicleDB db, IChannel chn, IFormat format)
			: base(db, chn, format)
        {
        }

        public static string CalcStdObdTroubleCode(byte[] buffer, int pos, int factor, int offset)
        {
            StringBuilder sb = new StringBuilder();
            int mode = buffer[pos * factor + offset] & 0xC0;
            int value1 = buffer[pos * factor + offset] & 0xFF;
            int value2 = buffer[pos * factor + offset + 1] & 0xFF;
            switch (mode)
            {
                case 0x00:
                    sb.Append(String.Format("P{0:X2}", value1));
                    break;
                case 0x40:
                    sb.Append(String.Format("C{0:X2}", value1));
                    break;
                case 0x80:
                    sb.Append(String.Format("B{0:X2}", value1));
                    break;
                case 0xC0:
                    sb.Append(String.Format("U{0:X2}", value1));
                    break;
                default:
                    break;
            }
            sb.Append(String.Format("{0:X2}", value2));
            return sb.ToString();
        }

        public abstract List<TroubleCodeItem> ReadCurrent();

        public abstract List<TroubleCodeItem> ReadHistory();

        public abstract void Clear();
    }
}


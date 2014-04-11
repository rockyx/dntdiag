using System;
using DNT.Diag.Attributes;
using DNT.Diag.Commbox;

namespace DNT.Diag.Channel
{
    public static class ChannelFactory
    {
        public static IChannel Create(Parameter param, ICommbox box, ProtocolType type)
        {
            if (box is Commbox.W80.W80Commbox)
            {
                return W80Create(param, box as Commbox.W80.W80Commbox, type);
            }
            throw new ArgumentException("Commbox");
        }

        private static IChannel W80Create(Parameter param, Commbox.W80.W80Commbox box, ProtocolType type)
        {
            switch (type)
            {
                case ProtocolType.MikuniECU200:
                    return new W80.MikuniECU200Channel(param, box);
                case ProtocolType.MikuniECU300:
                    return new W80.MikuniECU300Channel(param, box);
                case ProtocolType.ISO14230:
                    return new W80.ISO14230Channel(param, box);
                case ProtocolType.ISO9141_2:
                    return new W80.ISO9141Channel(param, box);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}


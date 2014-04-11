using System;

namespace DNT.Diag.Commbox
{
    public static class CommboxFactory
    {
        public static ICommbox Create()
        {
            switch (Settings.CommboxVersion)
            {
                case CommboxVersion.W80:
                    return new W80.W80Commbox();
                default:
                    return null;
            }
        }
    }
}


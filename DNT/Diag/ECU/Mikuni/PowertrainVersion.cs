using System;

namespace DNT.Diag.ECU.Mikuni
{
    public class PowertrainVersion
    {
        private string model;

        public string Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
            }
        }

        private string hardware;

        public string Hardware
        {
            get
            {
                return hardware;
            }
            set
            {
                hardware = value;
            }
        }

        private string software;

        public string Software
        {
            get
            {
                return software;
            }
            set
            {
                software = value;
            }
        }
    }
}


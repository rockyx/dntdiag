using System;
using DNT.Diag.Commbox;
using DNT.Diag.Commbox.W80;
using DNT.Diag.Attributes;

namespace DNT.Diag.Channel.W80
{
    internal abstract class Channel : AbstractChannel
    {
        private W80Commbox commbox;

        public Channel(Parameter param, W80Commbox commbox)
			: base(param, commbox)
        {
            this.commbox = commbox as W80Commbox;
        }

        protected new W80Commbox Commbox
        {
            get { return commbox; }
        }
    }
}


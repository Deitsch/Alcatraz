using RegistrationServer.Listener;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationServer.Spread
{
    public abstract class Operation
    {
        MessageListener listener;

        protected Operation(MessageListener listener)
        {
            this.listener = listener;
            listener.Receive += (sender, e) => HandleMessage(e.Message);
        }

        public abstract void HandleMessage(SpreadMessage message);
    }
}

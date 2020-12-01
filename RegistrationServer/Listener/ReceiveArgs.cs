using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationServer.Listener
{
    public class ReceiveArgs : EventArgs
    {
        public SpreadMessage Message { get; set; }

        public ReceiveArgs(SpreadMessage message)
        {
            Message = message;
        }
    }
}

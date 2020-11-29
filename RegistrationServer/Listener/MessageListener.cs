using RegistrationServer.Spread.Interface;
using spread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistrationServer.Listener
{
    public class MessageListener
    {
        public event EventHandler<ReceiveArgs> Receive = delegate { };

        public ISpreadConnection connection;

        public MessageListener(ISpreadConnection connection)
        {
            this.connection = connection;
        }

        public void Listen()
        {
            var spreadMessage = connection.Receive();
            Receive(this, new ReceiveArgs(spreadMessage));
        }
    }
}
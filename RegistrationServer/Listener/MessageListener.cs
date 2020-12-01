using RegistrationServer.Spread.Interface;
using System;

namespace RegistrationServer.Listener
{
    public class MessageListener
    {
        public event EventHandler<ReceiveArgs> Receive = delegate { };

        private readonly ISpreadConnectionWrapper connection;

        public MessageListener(ISpreadConnectionWrapper connection)
        {
            this.connection = connection;
        }

        public void Listen()
        {
            var spreadMessage = connection.SpreadConnection.Receive();
            Receive(this, new ReceiveArgs(spreadMessage));
        }
    }
}
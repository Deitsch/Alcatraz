using RegistrationServer.Listener;
using spread;

namespace RegistrationServer.Spread
{
    public abstract class Operation
    {
        protected Operation(MessageListener listener)
        {
            listener.Receive += (sender, e) => HandleMessage(e.Message);
        }

        public abstract void HandleMessage(SpreadMessage message);
    }
}

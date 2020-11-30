using RegistrationServer.Listener;
using spread;

namespace RegistrationServer.Spread
{
    public abstract class Operation
    {
        private readonly MessageListener listener;

        protected Operation(MessageListener listener)
        {
            this.listener = listener;
        }

        public abstract void HandleMessage(SpreadMessage message);

        public void AddListener()
        {
            listener.Receive += (sender, e) => HandleMessage(e.Message);
        }
    }
}

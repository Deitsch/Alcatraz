using spread;

namespace RegistrationServer.Spread.Interface
{
    public interface ISpreadConnection
    {
        public bool IsPrimary { get; }

        public spread.SpreadConnection spreadConnection { get; }

        public void Connect(string address, int port, string user, bool priority, bool groupMembership);

        public SpreadGroup JoinGroup(string groupName);

        public void SendMulticast(SpreadMulticastType type, string data);

        public SpreadMessage ReceiveMessage();

        public void Run();
        void MulticastLobbyInfoToPrimary(Proto.LobbyInfo request);
    }
}

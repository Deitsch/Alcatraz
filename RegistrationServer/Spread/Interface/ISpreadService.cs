using RegistrationServer.Spread.Enums;

namespace RegistrationServer.Spread.Interface
{
    public interface ISpreadService
    {
        public bool IsPrimary { get; }

        public string UserName { get; }

        string Port { get; }

        public int GroupMemberCounter { get; }

        public void SendMulticast(MulticastType type, byte[] data);

        public void SendMulticast(MulticastType type, string data);

        public void Run();
    }
}

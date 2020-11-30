using spread;

namespace RegistrationServer.Spread.Interface
{
    public interface ISpreadConnectionWrapper
    {
        public SpreadConnection SpreadConnection { get; }
        public SpreadGroup SpreadGroup { get; }
        public string UserName { get; }

        public void Connect(string address, int port, string user, bool priority, bool groupMembership);
    }
}

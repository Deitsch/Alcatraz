using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread.Enums;

namespace RegistrationServer.Spread
{
    public class SpreadDto
    {
        public OperationType Type { get; set; }

        public string OriginalSender { get; set; }

        public string LobbyId { get; set; }

        public NetworkPlayer Player { get; set; }
    }
}
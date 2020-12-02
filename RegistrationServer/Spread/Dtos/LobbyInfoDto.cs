using RegistrationServer.Lobby.Proto;

namespace RegistrationServer.Spread
{
    public class LobbyInfoDto
    {
        public string LobbyId { get; set; }

        public Google.Protobuf.Collections.RepeatedField<NetworkPlayer> Players { get; set; }
    }
}
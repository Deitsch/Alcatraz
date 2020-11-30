using RegistrationServer.Lobby.Proto;

namespace RegistrationServer.Spread
{
    class LobbyDto
    {
        public string OriginalSender { get; set; }
        public LobbyInfo LobbyInfo { get; set; }
    }
}

using RegistrationServer.Proto;
using RegistrationServer.utils;
using System.Text.Json;

namespace RegistrationServer.Spread
{
    public class LobbySpreadDto
    {
        public string OriginalSender { get; set; }
        public LobbyInfo LobbyInfo { get; set; }

        public LobbySpreadDto(byte[] data)
        {
            string jsonString = data.decodeToString();
            var lobby = JsonSerializer.Deserialize<LobbySpreadDto>(jsonString);
            OriginalSender = lobby.OriginalSender;
            LobbyInfo = lobby.LobbyInfo;
        }
    }
}

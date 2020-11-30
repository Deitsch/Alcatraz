using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread;
using spread;
using System.Text;
using System.Text.Json;

namespace RegistrationServer.utils
{
    public static class Utils
    { 
        public static string DecodeToString(this byte[] b)
            => Encoding.ASCII.GetString(b);

        public static byte[] EncodeToByteArray(this string s)
            => Encoding.ASCII.GetBytes(s);

        public static LobbyInfo GetLobby(this SpreadMessage msg)
            => JsonSerializer.Deserialize<LobbyDto>(msg.Data.DecodeToString()).LobbyInfo;

        public static string GetOriginalSender(this SpreadMessage msg)
            => JsonSerializer.Deserialize<LobbyDto>(msg.Data.DecodeToString()).OriginalSender;
    }
}

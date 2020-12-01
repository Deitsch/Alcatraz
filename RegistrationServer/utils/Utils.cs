using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Enums;
using spread;
using System;
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

        private static SpreadDto GetSpreadDto(this string jsonString)
            => JsonSerializer.Deserialize<SpreadDto>(jsonString);

        private static SpreadDto GetSpreadDto(this SpreadMessage msg)
            => msg.Data.DecodeToString().GetSpreadDto();

        public static string GetLobbyId(this SpreadMessage msg)
            => msg.GetSpreadDto().LobbyId;

        public static string GetOriginalSender(this SpreadMessage msg)
            => msg.GetSpreadDto().OriginalSender;

        public static NetworkPlayer GetPlayer(this SpreadMessage msg)
            => msg.GetSpreadDto().Player;

        public static string GetLobbyId(this string jsonString)
            => jsonString.GetSpreadDto().LobbyId;

        public static OperationType? GetOperationType(this SpreadMessage msg)
        {
            try
            {
                return JsonSerializer.Deserialize<SpreadDto>(msg.Data.DecodeToString()).Type;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

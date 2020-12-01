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

        private static SpreadDto ToSpreadDto(this string jsonString)
            => JsonSerializer.Deserialize<SpreadDto>(jsonString);

        private static SpreadDto ToSpreadDto(this SpreadMessage msg)
            => msg.Data.DecodeToString().ToSpreadDto();

        public static string LobbyId(this SpreadMessage msg)
            => msg.ToSpreadDto().LobbyId;

        public static string OriginalSender(this SpreadMessage msg)
            => msg.ToSpreadDto().OriginalSender;

        public static NetworkPlayer Player(this SpreadMessage msg)
            => msg.ToSpreadDto().Player;

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

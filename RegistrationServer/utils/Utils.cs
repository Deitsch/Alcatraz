using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Enums;
using spread;
using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
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

        public static SpreadDto ToSpreadDto(this SpreadMessage msg)
            => msg.Data.DecodeToString().ToSpreadDto();

        public static string LobbyId(this SpreadMessage msg)
            => msg.ToSpreadDto().LobbyId;

        public static string OriginalSender(this SpreadMessage msg)
            => msg.ToSpreadDto().OriginalSender;

        public static NetworkPlayer Player(this SpreadMessage msg)
            => msg.ToSpreadDto().Player;

        public static string IpWithPort(this SpreadMessage msg)
            => msg.ToSpreadDto().IpWithPort;

        public static LobbyInfoDto ToDto(this LobbyInfo lobbyInfo)
        {
            return new LobbyInfoDto
            {
                LobbyId = lobbyInfo.Id,
                Players = lobbyInfo.Players
            };
        }

        public static LobbyInfo ToLobbyInfo(this LobbyInfoDto dto)
        {
            var lobbyInfo = new LobbyInfo
            {
                Id = dto.LobbyId
            };
            lobbyInfo.Players.AddRange(dto.Players);

            return lobbyInfo;
        }

        public static OperationType? GetOperationType(this SpreadMessage msg)
        {
            try
            {
                return msg.ToSpreadDto().Type;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
    }
}

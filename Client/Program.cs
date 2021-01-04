using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Client.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Player = Client.Lobby.Proto.Player;
using PlayerState = Client.Lobby.Proto.PlayerState;

namespace Client
{
    public class Program
    {

        private static LobbyHandler _userInputHandler;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Write("Enter Player Name: ");
            var playerName = Console.ReadLine();

            // Ethernet: NetworkInterfaceType.Ethernet
            // Wireless: NetworkInterfaceType.Wireless80211
            var ip = GetLocalIPv4(NetworkInterfaceType.Ethernet);
            var port = new Random().Next(5050, 5999);

            CreateWebHostBuilder(args, ip ,port.ToString()).Build().RunAsync();

            var player = new Player
            {
                Ip = ip,
                Port = port,
                Name = playerName,
                PlayerState = PlayerState.Unknown,
            };
            _userInputHandler = new LobbyHandler(ip, 5000, player);
            _userInputHandler.HandleUserInput();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, string ip, string port) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://{ip}:{port}")
                .UseStartup<Startup>();

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
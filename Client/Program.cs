using System;
using System.Net;
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


            string hostname = Dns.GetHostName();
            IPHostEntry host = Dns.GetHostEntry(hostname);

            string ip = "";
            foreach (IPAddress address in host.AddressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = address.ToString();
                    break;
                }
            }
            var port = new Random().Next(5501, 5999);

            CreateWebHostBuilder(args, ip ,port.ToString()).Build().RunAsync();

            var player = new Player
            {
                Ip = ip,
                Port = port,
                Name = playerName,
                PlayerState = PlayerState.Unknown,
            };
            _userInputHandler = new LobbyHandler(player);
            _userInputHandler.HandleUserInput();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, string ip, string port) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://{ip}:{port}")
                .UseStartup<Startup>();
    }
}
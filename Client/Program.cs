using System;
using Client.Services;
using Grpc.Net.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Player = Client.Lobby.Proto.Player;
using PlayerState = Client.Lobby.Proto.PlayerState;

namespace Client
{
    public class Program
    {

        private static UserInputHandler _userInputHandler;

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Write("Enter Player Name: ");
            var playerName = Console.ReadLine();

            // we will use a static port but we need a virtual machine or smth
            Console.Write("Port: ");
            var port = Console.ReadLine();

            CreateWebHostBuilder(args, port).Build().RunAsync();

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress($"http://127.0.0.1:5001");

            var player = new Player
            {
                Ip = "127.0.0.1",
                //Ip = ip,
                Port = Convert.ToInt32(port),
                Name = playerName,
                PlayerState = PlayerState.Unknown,
            };

            _userInputHandler = new UserInputHandler(channel, player);
            _userInputHandler.HandleUserInput();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, string port = "5002") =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://localhost:{port}")
                .UseStartup<Startup>();

    }
}
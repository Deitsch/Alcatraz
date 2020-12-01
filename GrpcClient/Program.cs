using Grpc.Net.Client;
using GrpcClient.Lobby.Proto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using Player = GrpcClient.Lobby.Proto.Player;

namespace GrpcClient
{
    public class Program
    {

        private static UserInputHandler _userInputHandler;
        private static Game.Proto.Game.GameClient gameClient;


        public static async Task Main(string[] args)
        {
            //var alcatraz = new Alcatraz.Alcatraz();
            //alcatraz.showWindow();
            Console.Write("Enter your port: ");
            var port = Console.ReadLine();
            var webHost = CreateHostBuilder(args,port).Build();
            webHost.Start();


            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            gameClient = new Game.Proto.Game.GameClient(channel);
            Console.WriteLine($"args: {string.Join(",",args)}");

            Console.Write("Enter Player Name: ");
            var playerName = Console.ReadLine();
            var player = new Player
            {
                Ip = "127.0.0.1",
                Port = Convert.ToInt32(port),
                Name = playerName,
                PlayerState = PlayerState.Unknown,
            };
            
            _userInputHandler = new UserInputHandler(channel, player);
            _userInputHandler.HandleUserInput();

            await webHost.WaitForShutdownAsync();
        }


        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args, string port) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://localhost:{port}");
                });
    }
}

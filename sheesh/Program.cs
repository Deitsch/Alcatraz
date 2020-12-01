using System;
using System.Threading.Tasks;
//using System.Windows.Forms;
using Grpc.Net.Client;
using Client.Game.Proto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Client
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        ///

        private static LobbyHandler _lobbyHandler;
        private static Game.Proto.Game.GameClient gameClient;

        private static void Main(string[] args)
        {
            //Application.SetHighDpiMode(HighDpiMode.SystemAware);
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            //var alcatraz = new Alcatraz.Alcatraz();
            //alcatraz.init(2, 1);
            //alcatraz.showWindow();

            Console.Write("Enter your port: ");
            var port = Console.ReadLine();
            var webHost = CreateHostBuilder(args,port).Build();
            webHost.Start();

            
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            gameClient = new Client.Game.Proto.Game.GameClient(channel);

            Console.Write("Enter Player Name: ");
            var playerName = Console.ReadLine();
            var player = new Client.Lobby.Proto.Player
            {
                Ip = "127.0.0.1",
                Port = Convert.ToInt32(port),
                Name = playerName
            };

            _lobbyHandler = new LobbyHandler(channel, player);
            _lobbyHandler.HandleUserInput();
            
            webHost.WaitForShutdown();
        }

        public static IHostBuilder CreateHostBuilder(string[] args,string port) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://localhost:{port}");
                });
    }
}

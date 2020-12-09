using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Client.Lobby.Proto;
using Client.Services;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

using Player = Client.Lobby.Proto.Player;
using PlayerState = Client.Lobby.Proto.PlayerState;

namespace Client
{
    public class Program
    {
        public static Form MainForm { get; private set; }

        private static Lobby.Proto.Lobby.LobbyClient lobbyClient;
        private static Player _player;
        private static string _currentLobbyId;
        private static Random random;
        private static bool PlayerIsInLobby => _player.PlayerState == PlayerState.InLobby;
        private static bool PlayerIsInGame => GameService.PlayerState == Game.Proto.PlayerState.InGame;

        [STAThread]
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().RunAsync();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Alcatraz.Alcatraz a = new Alcatraz.Alcatraz();
            MainForm = a.getWindow();
            //Application.Run(MainForm);
            //a.showWindow();

            using var channel = GrpcChannel.ForAddress($"http://127.0.0.1:5001");
            var gameClient = new Game.Proto.Game.GameClient(channel);

            var playerName = "Player1";
            var player = new Player
            {
                Ip = "127.0.0.1",
                Port = Convert.ToInt32(80),
                Name = playerName,
                PlayerState = PlayerState.Unknown,
            };

            random = new Random();
            lobbyClient = new Lobby.Proto.Lobby.LobbyClient(channel);



            CreateLobby(player);

        }

        private static void CreateLobby(Player player)
        {
            try
            {
                if (true && true)
                {
                    var reply = lobbyClient.CreateLobby(new CreateLobbyRequest { Player = player });
                    _currentLobbyId = reply.Lobby.Id;
                    Console.WriteLine(
                        $"You created and joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
                    //_player.PlayerState = PlayerState.InLobby;

                }
                else
                {
                    Console.WriteLine("You are already in a lobby");
                }
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine($"ERROR: {rpcException.StatusCode} {rpcException.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
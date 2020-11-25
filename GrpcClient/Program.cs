using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using GrpcClient.Proto;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcClient
{
    public class Program
    {
        private static string helpText = 
                                $"Use following commands to interact with server:{Environment.NewLine}"+
                                $"0. Help{Environment.NewLine}"+
                                $"1. Get Lobbies{Environment.NewLine}"+
                                $"2. Create Lobby{Environment.NewLine}"+
                                $"3. Join Lobby{Environment.NewLine}"+
                                $"4. Leave Lobby{Environment.NewLine}"+
                                $"5. Exit";

        private static Lobby.LobbyClient client;
        private static Player player;

        public static async Task Main(string[] args)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            // The port number(5001) must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("http://localhost:5001");
            client = new Lobby.LobbyClient(channel);

            Console.Write("Enter Player Name: ");
            var playerName = Console.ReadLine();
            player = new Player
            {
                Ip = "127.0.0.1",
                Port = 5001,
                Name = playerName
            };

            Console.WriteLine(helpText);

            HandleUserInput();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            CreateHostBuilder(args).Build().Run();
        }

        private static void HandleUserInput() 
        {
            Console.Write("Input: ");
            var userInput = Console.ReadLine();
            while (userInput != "5")
            {
                switch (userInput)
                {
                    case "0":
                        Console.WriteLine(helpText);
                        break;
                    case "1":
                        GetLobbies();
                        break;
                    case "2":
                        CreateLobby(player);
                        break;
                    case "3":
                        Console.Write("LobbyId: ");
                        var lobbyId = Console.ReadLine();
                        JoinLobby(lobbyId, player);
                        break;
                    case "4":
                        Console.Write("LobbyId: ");
                        var lobId = Console.ReadLine();
                        LeaveLobby(lobId, player);
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
                Console.Write("Input: ");
                userInput = Console.ReadLine();
            }
        }

        private static void LeaveLobby(string lobbyId, Player player)
        {
            var reply = client.LeaveLobby(new LeaveLobbyRequest {LobbyId = lobbyId, Player = player});
            Console.WriteLine("You left lobby");
        }

        private static void GetLobbies() 
        {
            var reply = client.GetLobbies(new GetLobbiesRequest());
            foreach (var lobby in reply.Lobbies)
            {
                Console.WriteLine($"LobbyId: {lobby.Id} Players in Lobby: {lobby.Players.Count} PlayerNames: {string.Join(", ", lobby.Players.Select(x => x.Name))}");
            }
        }

        private static void CreateLobby(Player player) 
        {
            try
            {
                var reply = client.CreateLobby(new CreateLobbyRequest { Player = player });
                Console.WriteLine($"You created and joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }

        private static void JoinLobby(string lobbyId, Player player)
        {
            var reply = client.JoinLobby(new JoinLobbyRequest {LobbyId = lobbyId, Player = player});
            Console.WriteLine($"You joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
        }


        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

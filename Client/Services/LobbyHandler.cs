using System;
using System.Linq;
using Client.Lobby.Proto;
using Client.Controllers;
using PlayerState = Client.Lobby.Proto.PlayerState;
using Grpc.Net.Client;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Client.Services
{
    public class LobbyHandler
    {
        private static string helpText =
            $"Use following commands to interact with server:{Environment.NewLine}" +
            $"help -> Help{Environment.NewLine}" +
            $"get -> Get Lobbies{Environment.NewLine}" +
            $"create -> Create Lobby{Environment.NewLine}" +
            $"join -> Join Lobby{Environment.NewLine}" +
            $"leave -> Leave Lobby{Environment.NewLine}" +
            $"start -> Start Game{Environment.NewLine}" +
            $"exit -> Exit";
        private static Lobby.Proto.Lobby.LobbyClient lobbyClient;
        private static Player _player;
        private List<string> _addresses;
        private string currentServer => _addresses.First();

        private static string _currentLobbyId;
        private static bool PlayerIsInLobby => _player.PlayerState == PlayerState.InLobby;
        private static bool PlayerIsInGame => GameService.PlayerState == Game.Proto.PlayerState.InGame;

        public LobbyHandler(Player player)
        {
            _player = player;
            _addresses = GetServerAddressesFromFtp();
            if (_addresses.Count == 0)
                Environment.Exit(1);
            SetNextServer(true);
        }


        public void HandleUserInput()
        {
            Console.WriteLine(helpText);
            var userInput = Console.ReadLine();
            while (userInput != "exit")
            {
                switch (userInput)
                {
                    case "help":
                        Console.WriteLine(helpText);
                        break;
                    case "get":
                        GetLobbies();
                        break;
                    case "create":
                        CreateLobby(_player);
                        break;
                    case "join":
                        JoinLobby(_player);
                        break;
                    case "leave":
                        LeaveLobby(_currentLobbyId, _player);
                        break;
                    case "start":
                        StartGame(_currentLobbyId);
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }
                userInput = Console.ReadLine();
            }
        }

        private void StartGame(string lobbyId)
        {
            try
            {
                lobbyClient.RequestGameStart(new RequestGameStartRequest { LobbyId = lobbyId });
                Console.WriteLine($"Game of Lobby {lobbyId} started!");
            }
            catch (Grpc.Core.RpcException e)
            {
                Console.Error.WriteLine($"Registration Server {currentServer} not found");
                SetNextServer();
                GetLobbies();
            }
        }

        private void LeaveLobby(string lobbyId, Player player)
        {
            try
            {
                if (PlayerIsInLobby && !PlayerIsInGame)
                {
                    var reply = lobbyClient.LeaveLobby(new LeaveLobbyRequest { LobbyId = lobbyId, Player = player });
                    _currentLobbyId = null;
                    _player.PlayerState = PlayerState.Unknown;

                    Console.WriteLine("You left lobby");
                }
                else
                {
                    Console.WriteLine("You are not in a lobby");
                }
            }
            catch (Grpc.Core.RpcException e)
            {
                Console.Error.WriteLine($"Registration Server {currentServer} not found");
                SetNextServer();
                GetLobbies();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void GetLobbies()
        {
            try
            {
                var reply = lobbyClient.GetLobbies(new GetLobbiesRequest());
                if (reply.Lobbies.Count == 0)
                {
                    Console.WriteLine("No Lobbies found");
                }

                foreach (var lobby in reply.Lobbies)
                {
                    Console.WriteLine(
                        $"LobbyId: {lobby.Id} Players in Lobby: {lobby.Players.Count} PlayerNames: {string.Join(", ", lobby.Players.Select(x => x.Name))}");
                }
            }
            catch(Grpc.Core.RpcException e)
            {
                Console.Error.WriteLine($"Registration Server {currentServer} not found");
                SetNextServer();
                GetLobbies();
            }
        }

        private void CreateLobby(Player player)
        {
            try
            {
                if (!PlayerIsInLobby && !PlayerIsInGame)
                {
                    var reply = lobbyClient.CreateLobby(new CreateLobbyRequest { Player = player });
                    _currentLobbyId = reply.Lobby.Id;
                    Console.WriteLine(
                        $"You created and joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
                    _player.PlayerState = PlayerState.InLobby;

                }
                else
                {
                    Console.WriteLine("You are already in a lobby");
                }
            }
            catch (Grpc.Core.RpcException e)
            {
                Console.Error.WriteLine($"Registration Server {currentServer} not found");
                SetNextServer();
                GetLobbies();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void JoinLobby(Player player)
        {
            
            try
            {
                if (!PlayerIsInLobby && !PlayerIsInGame)
                {
                    Console.Write("LobbyId: ");
                    var lobbyId = Console.ReadLine();
                    var reply = lobbyClient.JoinLobby(new JoinLobbyRequest { LobbyId = lobbyId, Player = player });
                    _currentLobbyId = reply.Lobby.Id;
                    Console.WriteLine(
                        $"You joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
                    _player.PlayerState = PlayerState.InLobby;
                }
                else
                {
                    Console.WriteLine("You are already in a lobby");
                }
            }
            catch (Grpc.Core.RpcException e)
            {
                Console.Error.WriteLine($"Registration Server {currentServer} not found");
                SetNextServer();
                GetLobbies();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void SetNextServer(bool initialSet = false)
        {
            if(!initialSet)
                _addresses.RemoveAt(0);
            if (_addresses.Count == 0)
            {
                System.Threading.Thread.Sleep(10000);
                _addresses = GetServerAddressesFromFtp();
            }
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress($"http://{currentServer}");
            lobbyClient = new Lobby.Proto.Lobby.LobbyClient(channel);
        }

        public static List<string> GetServerAddressesFromFtp()
        {
            const string USERNAME = "alcatraz";
            const string PASSWORD = "campus09";
            const string FTP_URL = "ftp://alcatraz.bplaced.net/";
            const string IP_ADDRESSES_FILENAME = "IpAddresses.txt";
            const string IP_ADDRESSES_DELIMITER = ",";

            List<string> input = new List<string>();
            var client = new WebClient
            {
                Credentials = new NetworkCredential(USERNAME, PASSWORD)
            };

            Stream myStream = client.OpenRead(FTP_URL + IP_ADDRESSES_FILENAME);
            StreamReader sr = new StreamReader(myStream);
            while (sr.Peek() >= 0)
            {
                input.Add(sr.ReadLine());
            }
            myStream.Close();

            List<string> addresses = input.First().Split(IP_ADDRESSES_DELIMITER).ToList();

            Console.WriteLine("Registered Servers: " + input.First());
            return addresses;
        }
    }
}

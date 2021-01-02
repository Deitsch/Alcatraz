using System;
using System.Linq;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using Client.Game.Proto;
using Client.Lobby.Proto;
using Client.Controllers;
using PlayerState = Client.Lobby.Proto.PlayerState;

namespace Client.Services
{
    public class UserInputHandler
    {
        private static string helpText =
            $"Use following commands to interact with server:{Environment.NewLine}" +
            $"help -> Help{Environment.NewLine}" +
            $"get -> Get Lobbies{Environment.NewLine}" +
            $"create -> Create Lobby{Environment.NewLine}" +
            $"join -> Join Lobby{Environment.NewLine}" +
            $"leave -> Leave Lobby{Environment.NewLine}" +
            $"start -> Start Game{Environment.NewLine}" +
            $"move -> Make a move{Environment.NewLine}" +
            $"exit -> Exit";
        private static Lobby.Proto.Lobby.LobbyClient lobbyClient;
        private static Player _player;
        private static string _currentLobbyId;
        private static bool PlayerIsInLobby => _player.PlayerState == PlayerState.InLobby;
        private static bool PlayerIsInGame => GameService.PlayerState == Game.Proto.PlayerState.InGame;

        public UserInputHandler(ChannelBase channel, Player player)
        {
            _player = player;
            lobbyClient = new Lobby.Proto.Lobby.LobbyClient(channel);
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

        private static void StartGame(string lobbyId)
        {
            try
            {
                lobbyClient.RequestGameStart(new RequestGameStartRequest { LobbyId = lobbyId });
                Console.WriteLine($"Game of Lobby {lobbyId} started!");
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine($"ERROR: {rpcException.StatusCode} {rpcException.Message}");
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
            catch (RpcException rpcException)
            {
                Console.WriteLine($"ERROR: {rpcException.StatusCode} {rpcException.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void GetLobbies()
        {
            var reply = lobbyClient.GetLobbies(new GetLobbiesRequest());
            if(reply.Lobbies.Count == 0)
            {
                Console.WriteLine("No Lobbies found");
            }

            foreach (var lobby in reply.Lobbies)
            {
                Console.WriteLine(
                    $"LobbyId: {lobby.Id} Players in Lobby: {lobby.Players.Count} PlayerNames: {string.Join(", ", lobby.Players.Select(x => x.Name))}");
            }
        }

        private static void CreateLobby(Player player)
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
            catch (RpcException rpcException)
            {
                Console.WriteLine($"ERROR: {rpcException.StatusCode} {rpcException.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

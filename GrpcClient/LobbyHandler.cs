using System;
using System.Linq;
using Grpc.Core;
using GrpcClient.Lobby.Proto;

namespace GrpcClient
{
    public class LobbyHandler
    {
        private static string helpText =
            $"Use following commands to interact with server:{Environment.NewLine}" +
            $"plshelp -> Help{Environment.NewLine}" +
            $"get -> Get Lobbies{Environment.NewLine}" +
            $"create -> Create Lobby{Environment.NewLine}" +
            $"join -> Join Lobby{Environment.NewLine}" +
            $"leave -> Leave Lobby{Environment.NewLine}" +
            $"start -> Start Game{Environment.NewLine}" +
            $"exit -> Exit";
        private static Lobby.Proto.Lobby.LobbyClient lobbyClient;
        private static Player _player;
        private static string _currentLobbyId;
        private static bool PlayerIsInLobby => _currentLobbyId != null;

        public LobbyHandler(ChannelBase channel, Player player)
        {
            _player = player;
            lobbyClient = new Lobby.Proto.Lobby.LobbyClient(channel);
        }


        public void HandleUserInput()
        {
            Console.WriteLine(helpText);
            Console.Write("Input: ");
            var userInput = Console.ReadLine();
            while (userInput != "exit")
            {
                switch (userInput)
                {
                    case "plshelp":
                        Console.WriteLine(helpText);
                        break;
                    case "get":
                        GetLobbies();
                        break;
                    case "create":
                        CreateLobby(_player);
                        break;
                    case "join":
                        Console.Write("LobbyId: ");
                        var lobbyId = Console.ReadLine();
                        JoinLobby(lobbyId, _player);
                        break;
                    case "leave":
                        LeaveLobby(_currentLobbyId, _player);
                        break;
                    case "start":
                        StartGame(_currentLobbyId);
                        break;
                    case "move":
                        DoMove();
                        break;
                    default:
                        Console.WriteLine("Invalid input");
                        break;
                }

                Console.Write("Input: ");
                userInput = Console.ReadLine();
            }
        }

        private void DoMove()
        {
            ;
        }

        private static void StartGame(string lobbyId)
        {
            lobbyClient.RequestGameStart(new RequestGameStartRequest { LobbyId = lobbyId });
            Console.WriteLine($"Game of Lobby {lobbyId} started!");
            //todo delete/flag lobby
        }

        private void LeaveLobby(string lobbyId, Player player)
        {
            try
            {
                if (PlayerIsInLobby)
                {
                    var reply = lobbyClient.LeaveLobby(new LeaveLobbyRequest { LobbyId = lobbyId, Player = player });
                    _currentLobbyId = null;
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
                if (!PlayerIsInLobby)
                {
                    var reply = lobbyClient.CreateLobby(new CreateLobbyRequest { Player = player });
                    _currentLobbyId = reply.Lobby.Id;
                    Console.WriteLine(
                        $"You created and joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
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

        private void JoinLobby(string lobbyId, Player player)
        {
            try
            {
                if (!PlayerIsInLobby)
                {
                    var reply = lobbyClient.JoinLobby(new JoinLobbyRequest { LobbyId = lobbyId, Player = player });
                    _currentLobbyId = reply.Lobby.Id;
                    Console.WriteLine(
                        $"You joined Lobby {reply.Lobby.Id}, Current Players: {string.Join(", ", reply.Lobby.Players.Select(x => x.Name))}");
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
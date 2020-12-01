using System;
using System.Linq;
using System.Threading;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcClient.Game.Proto;
using GrpcClient.Lobby.Proto;
using GrpcClient.Services;
using PlayerState = GrpcClient.Lobby.Proto.PlayerState;

namespace GrpcClient
{
    public class UserInputHandler
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
        private static Random random;
        private static bool PlayerIsInLobby => _player.PlayerState == PlayerState.InLobby;
        private static bool PlayerIsInGame => GameService.PlayerState == Game.Proto.PlayerState.InGame;

        public UserInputHandler(ChannelBase channel, Player player)
        {
            _player = player;
            random = new Random();
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
                        JoinLobby(_player);
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
                userInput = Console.ReadLine();
            }
        }

        private void DoMove()
        {
            if (PlayerIsInGame)
            {
                if (GameService.ItsMyTurn)
                {
                    var makeMove = new MakeMoveRequest
                    {
                        MoveInfo = new MoveInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            PlayerName = _player.Name,
                            Prisoner = new Prisoner {OldPoint = null, NewPoint = new Point {X = random.Next(0,10), Y = random.Next(0, 10) },}
                        }
                    };
                    MakeReliableMove(makeMove);
                    Console.WriteLine("You made a move!");

                    var nextPlayer = GameService.NetworkPlayers[(GameService.Index + 1) % GameService.NetworkPlayers.Count];
                    SetNextPlayerReliable(nextPlayer);
                }
                else
                {
                    Console.WriteLine("It's not your turn");
                }
            }
            else
            {
                Console.WriteLine("You are not in a game!");
            }
        }

        private static void SetNextPlayerReliable(NetworkPlayer nextPlayer)
        {
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                try
                {
                    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                    using var c = GrpcChannel.ForAddress($"http://{nextPlayer.Ip}:{nextPlayer.Port}");
                    var gClient = new Game.Proto.Game.GameClient(c);
                    gClient.SetCurrentPlayer(new SetCurrentPlayerRequest());
                    GameService.ItsMyTurn = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    allGood = false;
                    Thread.Sleep(1000);
                }
            }

        }

        private static void MakeReliableMove(MakeMoveRequest makeMove)
        {
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                for (var index = 0; index < GameService.NetworkPlayers.Count; index++)
                {
                    var player = GameService.NetworkPlayers[index];
                    try
                    {
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport",
                            true);
                        var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                        using (channel)
                        {
                            var gameClient = new Game.Proto.Game.GameClient(channel);
                            gameClient.MakeMove(makeMove);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        allGood = false;
                        Thread.Sleep(1000);
                        break;
                    }
                }
            }
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
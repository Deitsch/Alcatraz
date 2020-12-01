using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using RegistrationServer.Game.Proto;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RegistrationServer.Services
{
    public class GameService : Game.Proto.Game.GameBase
    {
        private readonly ILogger<GameService> _logger;

        public GameService(ILogger<GameService> logger)
        {
            _logger = logger;
        }


        public void StartGame(LobbyInfo lobby)
        {
            var gameInfo = new GameInfo();
            foreach (var player in lobby.Players)
            {
                gameInfo.Players.Add(GetPlayer(player));
            }

            InitGameReliable(lobby, gameInfo);
            var firstPlayer = gameInfo.Players.First();
            SetNextPlayerReliable(firstPlayer);
            Console.WriteLine("Game started!");

            //var lobbyToDelete = LobbyService.Lobbies.Single(l => l.Id == lobby.Id);
            //LobbyService.Lobbies.Remove(lobbyToDelete);
        }

        private static void InitGameReliable(LobbyInfo lobby, GameInfo gameInfo)
        {
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                for (var index = 0; index < lobby.Players.Count; index++)
                {
                    var player = lobby.Players[index];
                    try
                    {
                        gameInfo.Index = index;
                        var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                        using (channel)
                        {
                            var gameClient = new Game.Proto.Game.GameClient(channel);
                            gameClient.InitGame(new InitGameRequest {GameInfo = gameInfo});
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Some Player did not respond -> retry in 1000 ms");
                        allGood = false;
                        Thread.Sleep(1000);
                        break;
                    }
                }
            }
        }


        private static void SetNextPlayerReliable(Game.Proto.NetworkPlayer firstPlayer)
        {
            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                try
                {
                    
                    using var c = GrpcChannel.ForAddress($"http://{firstPlayer.Ip}:{firstPlayer.Port}");
                    var gClient = new Game.Proto.Game.GameClient(c);
                    gClient.SetCurrentPlayer(new SetCurrentPlayerRequest());
                }
                catch (Exception)
                {
                    Console.WriteLine("Waiting for first Player to respond -> retry in 1000 ms");
                    allGood = false;
                    Thread.Sleep(1000);
                }
            }
        }

        private RegistrationServer.Game.Proto.NetworkPlayer GetPlayer(Lobby.Proto.NetworkPlayer lobbyPlayer)
        {
            return new Game.Proto.NetworkPlayer
            {
                Name = lobbyPlayer.Name,
                Ip = lobbyPlayer.Ip,
                Port = lobbyPlayer.Port,
            };
        }
    }
}

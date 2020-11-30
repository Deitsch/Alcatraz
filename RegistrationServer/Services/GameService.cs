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
        private readonly ISpreadConn spreadConn;


        public GameService(ILogger<GameService> logger, ISpreadConn spreadConn)
        {
            _logger = logger;
            this.spreadConn = spreadConn;
        }


        public void StartGame(LobbyInfo lobby)
        {
            var gameInfo = new GameInfo();
            gameInfo.Id = Guid.NewGuid().ToString();
            var gameClientList = new List<Game.Proto.Game.GameClient>();
            foreach (var player in lobby.Players)
            {
                gameInfo.Players.Add(GetPlayer(player));
                //var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                //gameClientList.Add(new Game.Proto.Game.GameClient(channel));
            }

            var allGood = false;
            while (!allGood)
            {
                allGood = true;
                foreach (var player in lobby.Players)
                {
                    try
                    {
                        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                        var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                        using (channel)
                        {
                            var gameClient = new Game.Proto.Game.GameClient(channel);
                            gameClient.InitGame(new InitGameRequest {GameInfo = gameInfo});
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
            var firstPlayer = gameInfo.Players.First();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var c = GrpcChannel.ForAddress($"http://{firstPlayer.Ip}:{firstPlayer.Port}");
            var gClient = new Game.Proto.Game.GameClient(c);
            gClient.StartGame(new StartGameRequest());

        }

        private RegistrationServer.Game.Proto.Player GetPlayer(Lobby.Proto.Player lobbyPlayer)
        {
            return new Game.Proto.Player
            {
                Name = lobbyPlayer.Name,
                Ip = lobbyPlayer.Ip,
                Port = lobbyPlayer.Port,
            };
        }
    }
}

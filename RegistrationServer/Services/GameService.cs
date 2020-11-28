using System;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Game.Proto;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Grpc.Net.Client;

namespace RegistrationServer
{
    public class GameService : Game.Proto.Game.GameBase
    {
        public LobbyService LobbyService { get; }
        private readonly ILogger<GameService> _logger;
        private readonly ISpreadConn spreadConn;


        public GameService(ILogger<GameService> logger, LobbyService lobbyService, ISpreadConn spreadConn)
        {
            LobbyService = lobbyService;
            _logger = logger;
            this.spreadConn = spreadConn;
        }

        public override Task<RequestGameStartResponse> RequestGameStart(RequestGameStartRequest request, ServerCallContext context)
        {
            var lobby = LobbyService.Lobbies.SingleOrDefault(l => l.Id == request.LobbyId);
            if (lobby == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            }

            if (lobby.Players.Count < 2)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Lobby with id {lobby.Id} has not enough players to start"));
            }

            StartGame(lobby);
            return Task.FromResult(new RequestGameStartResponse());
        }

        private void StartGame(LobbyInfo lobby)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var gameInfo = new GameInfo();
            gameInfo.Id = Guid.NewGuid().ToString();
            foreach (var player in lobby.Players)
            {
                gameInfo.Players.Add(GetPlayer(player));
            }
            foreach (var player in lobby.Players)
            {
                using var channel = GrpcChannel.ForAddress($"http://{player.Ip}:{player.Port}");
                var client = new Game.Proto.Game.GameClient(channel);
                client.StartGame(new StartGameRequest {GameInfo = gameInfo});
            }
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

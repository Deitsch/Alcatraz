using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Spread.Interface;

namespace RegistrationServer.Services
{
    public class LobbyService : Lobby.Proto.Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> _logger;
        private readonly ISpreadConn _spreadConn;
        private readonly GameService _gameService;
        private readonly List<LobbyInfo> _lobbies;

        public LobbyService(ILogger<LobbyService> logger, ISpreadConn spreadConn, GameService gameService)
        {
            _logger = logger;
            _spreadConn = spreadConn;
            _gameService = gameService;
            _lobbies = new List<LobbyInfo>();
        }

        public override Task<GetLobbiesResponse> GetLobbies(GetLobbiesRequest request, ServerCallContext context)
        {
            var response = new GetLobbiesResponse();
            foreach (var lobby in _lobbies)
            {
                response.Lobbies.Add(lobby);
            }
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
            var lobbyInfo = new LobbyInfo
            {
                Id = Guid.NewGuid().ToString(),
            };
            lobbyInfo.Players.Add(request.Player);
            _lobbies.Add(lobbyInfo);
            response.Lobby = lobbyInfo;
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
            var lobbyToJoin = _lobbies.Single(l => l.Id == request.LobbyId);
            if (lobbyToJoin == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            }

            if (lobbyToJoin.Players.Any(p => p.Name == request.Player.Name))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists,
                    $"A player with that name is already in lobby {lobbyToJoin.Id}"));
            }

            if (lobbyToJoin.Players.Count == 4)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    $"Lobby with id {lobbyToJoin.Id} is already full"));
            }
            lobbyToJoin.Players.Add(request.Player);
            _spreadConn.SendMessage($"New Player: {request.Player.Name}");
            response.Lobby = lobbyToJoin;
            return Task.FromResult(response);
        }

        public override Task<LeaveLobbyResponse> LeaveLobby(LeaveLobbyRequest request, ServerCallContext context)
        {
            var response = new LeaveLobbyResponse();
            var lobbyToLeave = _lobbies.SingleOrDefault(l => l.Id == request.LobbyId);
            if (lobbyToLeave == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            }

            if (!lobbyToLeave.Players.Remove(request.Player))
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Player {request.Player.Name} not found in Lobby {request.LobbyId}"));
            }

            if (lobbyToLeave.Players.Count == 0)
            {
                _lobbies.Remove(lobbyToLeave);
            }
            return Task.FromResult(response);
        }

        public override Task<RequestGameStartResponse> RequestGameStart(RequestGameStartRequest request, ServerCallContext context)
        {
            var lobby = _lobbies.SingleOrDefault(l => l.Id == request.LobbyId);
            if (lobby == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            }

            //if (lobby.Players.Count < 2)
            //{
            //    throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Lobby with id {lobby.Id} has not enough players to start"));
            //}

            _gameService.StartGame(lobby);
            return Task.FromResult(new RequestGameStartResponse());
        }
    }
}

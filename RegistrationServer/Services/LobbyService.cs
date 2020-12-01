using Grpc.Core;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Repositories;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegistrationServer.Services
{
    public class LobbyService : Lobby.Proto.Lobby.LobbyBase
    {
        private readonly ISpreadService spreadService;
        private readonly GameService gameService;
        private readonly LobbyRepository lobbyRepository;
        private readonly CreateLobbyOperation createLobbyOperation;
        private readonly GetLobbiesOperation getLobbiesOperation;
        private readonly JoinLobbyOperation joinLobbyOperation;
        private readonly LeaveLobbyOperation leaveLobbyOperation;
        private readonly DeleteLobbyOperation deleteLobbyOperation;

        public LobbyService(
            ISpreadService spreadService,
            GameService gameService,
            LobbyRepository lobbyRepository,
            CreateLobbyOperation createLobbyOperation,
            GetLobbiesOperation getLobbiesOperation,
            JoinLobbyOperation joinLobbyOperation,
            LeaveLobbyOperation leaveLobbyOperation,
            DeleteLobbyOperation deleteLobbyOperation)
        {
            this.spreadService = spreadService;
            this.gameService = gameService;
            this.lobbyRepository = lobbyRepository;
            this.createLobbyOperation = createLobbyOperation;
            this.getLobbiesOperation = getLobbiesOperation;
            this.joinLobbyOperation = joinLobbyOperation;
            this.leaveLobbyOperation = leaveLobbyOperation;
            this.deleteLobbyOperation = deleteLobbyOperation;
        }

        public override Task<GetLobbiesResponse> GetLobbies(GetLobbiesRequest request, ServerCallContext context)
        {
            var getLobbiesResponse = new GetLobbiesResponse();
            List<LobbyInfo> lobbies;

            try
            {
                lobbies = getLobbiesOperation.Execute();
            }
            catch (Exception e)
            {
                throw e;
            }

            getLobbiesResponse.Lobbies.AddRange(lobbies);
            return Task.FromResult(getLobbiesResponse);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            LobbyInfo lobby;
            string lobbyId = Guid.NewGuid().ToString();

            var spreadDto = new SpreadDto
            {
                Type = OperationType.CreateLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = lobbyId,
                Player = request.Player
            };

            try
            {
                createLobbyOperation.Execute(spreadDto, OperationType.CreateLobby);
                lobby = lobbyRepository.FindById(lobbyId);
            }
            catch (Exception e)
            {
                throw e;
            }

            var joinLobbyResponse = new JoinLobbyResponse
            {
                Lobby = lobby
            };
            return Task.FromResult(joinLobbyResponse);
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            LobbyInfo lobby;

            var spreadDto = new SpreadDto
            {
                Type = OperationType.JoinLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = request.LobbyId,
                Player = request.Player
            };

            try
            {
                joinLobbyOperation.Execute(spreadDto, OperationType.JoinLobby);
                lobby = lobbyRepository.FindById(request.LobbyId);
            }
            catch (Exception e)
            {
                throw e;
            }

            var joinLobbyResponse = new JoinLobbyResponse
            {
                Lobby = lobby
            };
            return Task.FromResult(joinLobbyResponse);
        }

        public override Task<LeaveLobbyResponse> LeaveLobby(LeaveLobbyRequest request, ServerCallContext context)
        {
            var spreadDto = new SpreadDto
            {
                Type = OperationType.LeaveLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = request.LobbyId,
                Player = request.Player
            };

            try
            {
                leaveLobbyOperation.Execute(spreadDto, OperationType.LeaveLobby);
            }
            catch (Exception e)
            {
                throw e;
            }

            return Task.FromResult(new LeaveLobbyResponse());
        }

        public override Task<RequestGameStartResponse> RequestGameStart(RequestGameStartRequest request, ServerCallContext context)
        {
            var lobbyId = request.LobbyId;
            var lobby = lobbyRepository.FindById(lobbyId);

            if (lobby == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {lobbyId} not found"));
            }
            if (lobby.Players.Count < 2)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Lobby with id {lobbyId} has not enough players to start"));
            }

            gameService.StartGame(lobby);

            var spreadDto = new SpreadDto
            {
                Type = OperationType.LeaveLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = lobbyId
            };

            try
            {
                deleteLobbyOperation.Execute(spreadDto, OperationType.RequestGameStart);
            }
            catch (Exception e)
            {
                throw e;
            }

            return Task.FromResult(new RequestGameStartResponse());
        }
    }
}

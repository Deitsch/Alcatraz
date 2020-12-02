using Grpc.Core;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Repositories;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using System;
using System.Linq;
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
            var lobbies = getLobbiesOperation.Execute();

            var getLobbiesResponse = new GetLobbiesResponse();
            getLobbiesResponse.Lobbies.AddRange(lobbies);
            return Task.FromResult(getLobbiesResponse);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            string lobbyId = Guid.NewGuid().ToString();

            var spreadDto = new SpreadDto
            {
                Type = OperationType.CreateLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = lobbyId,
                Player = request.Player
            };

            createLobbyOperation.Execute(spreadDto);

            var joinLobbyResponse = new JoinLobbyResponse
            {
                Lobby = lobbyRepository.FindById(lobbyId)
            };
            return Task.FromResult(joinLobbyResponse);
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var lobbyToJoin = lobbyRepository.FindById(request.LobbyId);

            if (lobbyToJoin == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Lobby with id {request.LobbyId} not found"));
            }
            if (lobbyToJoin.Players.Any(p => p.Name == request.Player.Name))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists,
                    $"A player with that name is already in lobby {request.LobbyId}"));
            }
            if (lobbyToJoin.Players.Count == 4)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    $"Lobby with id {request.LobbyId} is already full"));
            }

            var spreadDto = new SpreadDto
            {
                Type = OperationType.JoinLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = request.LobbyId,
                Player = request.Player
            };

            joinLobbyOperation.Execute(spreadDto);

            var joinLobbyResponse = new JoinLobbyResponse
            {
                Lobby = lobbyRepository.FindById(request.LobbyId)
            };
            return Task.FromResult(joinLobbyResponse);
        }

        public override Task<LeaveLobbyResponse> LeaveLobby(LeaveLobbyRequest request, ServerCallContext context)
        {
            var lobbyToLeave = lobbyRepository.FindById(request.LobbyId);
            
            if (lobbyToLeave == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Lobby with id {request.LobbyId} not found"));
            }
            if (!PlayerInLobby(lobbyToLeave, request.Player))
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Player {request.Player.Name} not found in Lobby {request.LobbyId}"));
            }

            var spreadDto = new SpreadDto
            {
                Type = OperationType.LeaveLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = request.LobbyId,
                Player = request.Player
            };

            leaveLobbyOperation.Execute(spreadDto);

            return Task.FromResult(new LeaveLobbyResponse());
        }

        private bool PlayerInLobby(LobbyInfo lobbyToLeave, NetworkPlayer player)
        {
            return lobbyToLeave.Players.Any(p => p.Name == player.Name);
        }

        public override Task<RequestGameStartResponse> RequestGameStart(RequestGameStartRequest request, ServerCallContext context)
        {
            var lobby = lobbyRepository.FindById(request.LobbyId);

            if (lobby == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            }
            if (lobby.Players.Count < 2)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Lobby with id {request.LobbyId} has not enough players to start"));
            }

            gameService.StartGame(lobby);

            var spreadDto = new SpreadDto
            {
                Type = OperationType.DeleteLobby,
                OriginalSender = spreadService.UserName,
                LobbyId = request.LobbyId
            };

            deleteLobbyOperation.Execute(spreadDto);

            return Task.FromResult(new RequestGameStartResponse());
        }
    }
}

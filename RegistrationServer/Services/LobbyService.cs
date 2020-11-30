using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Listener;
using RegistrationServer.Lobby.Proto;
using RegistrationServer.Repositories;
using RegistrationServer.Spread;
using RegistrationServer.Spread.Interface;
using System;
using System.Threading.Tasks;

namespace RegistrationServer
{
    public class LobbyService : Lobby.Proto.Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> logger;
        private readonly ISpreadService spreadService;
        private readonly MessageListener listener;
        private readonly LobbyRepository lobbyRepository;
        private readonly CreateLobbyOperation createLobbyOperation;

        public LobbyService(ILogger<LobbyService> logger, ISpreadService spreadService, MessageListener listener, LobbyRepository lobbyRepository, CreateLobbyOperation createLobbyOperation)
        {
            this.logger = logger;
            this.spreadService = spreadService;
            this.listener = listener;
            this.lobbyRepository = lobbyRepository;
            this.createLobbyOperation = createLobbyOperation;
        }

        public override Task<GetLobbiesResponse> GetLobbies(GetLobbiesRequest request, ServerCallContext context)
        {
            var response = new GetLobbiesResponse();

            response.Lobbies.AddRange(lobbyRepository.GetLobbies());
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            var lobbyInfo = new LobbyInfo
            {
                Id = Guid.NewGuid().ToString()
            };
            lobbyInfo.Players.Add(request.Player);

            try
            {
                createLobbyOperation.Execute(lobbyInfo);

                JoinLobbyResponse joinLobbyResponse = new JoinLobbyResponse
                {
                    Lobby = lobbyInfo
                };
                return Task.FromResult(joinLobbyResponse);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
            //var lobbyToJoin = lobbies.Single(l => l.Id == request.LobbyId);
            //if (lobbyToJoin == null)
            //{
            //    throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            //}

            //if (lobbyToJoin.Players.Any(p => p.Name == request.Player.Name))
            //{
            //    throw new RpcException(new Status(StatusCode.AlreadyExists,
            //        $"A player with that name is already in lobby {lobbyToJoin.Id}"));
            //}

            //if (lobbyToJoin.Players.Count == 4)
            //{
            //    throw new RpcException(new Status(StatusCode.FailedPrecondition,
            //        $"Lobby with id {lobbyToJoin.Id} is already full"));
            //}
            //lobbyToJoin.Players.Add(request.Player);
            //spreadConn.SendMessage($"New Player: {request.Player.Name}");
            //response.Lobby = lobbyToJoin;
            return Task.FromResult(response);
        }

        public override Task<LeaveLobbyResponse> LeaveLobby(LeaveLobbyRequest request, ServerCallContext context)
        {
            var response = new LeaveLobbyResponse();
            //var lobbyToLeave = lobbies.SingleOrDefault(l => l.Id == request.LobbyId);
            //if (lobbyToLeave == null)
            //{
            //    throw new RpcException(new Status(StatusCode.NotFound, $"Lobby with id {request.LobbyId} not found"));
            //}

            //if (!lobbyToLeave.Players.Remove(request.Player))
            //{
            //    throw new RpcException(new Status(StatusCode.NotFound,
            //        $"Player {request.Player.Name} not found in Lobby {request.LobbyId}"));
            //}

            //if (lobbyToLeave.Players.Count == 0)
            //{
            //    lobbies.Remove(lobbyToLeave);
            //}
            return Task.FromResult(response);
        }
    }
}

using System.Threading.Tasks;
using Grpc.Core;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Proto;
using System;
using RegistrationServer.Repositories;
using RegistrationServer.Spread;
using RegistrationServer.Listener;

namespace RegistrationServer
{
    public class LobbyService : Lobby.LobbyBase
    {
        private readonly ISpreadService spreadService;
        private readonly MessageListener listener;
        private readonly LobbyRepository lobbyRepository;

        public LobbyService(ISpreadService spreadService, MessageListener listener, LobbyRepository lobbyRepository)
        {
            this.spreadService = spreadService;
            this.lobbyRepository = lobbyRepository;
            this.listener = listener;
        }

        public override Task<GetLobbiesResponse> GetLobbies(GetLobbiesRequest request, ServerCallContext context)
        {
            var response = new GetLobbiesResponse();

            response.Lobbies.AddRange(lobbyRepository.GetLobbies());
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            LobbyInfo lobbyInfo = GetLobbyInfoFromCreateLobbyRequest(request);

            CreateLobbyOperation createLobbyOperation = new CreateLobbyOperation(listener, spreadService, lobbyRepository);

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
            spreadService.SendMulticast(MulticastType.NewPlayerJoined, $"New Player: {request.PlayerName}");
            return Task.FromResult(response);
        }

        private LobbyInfo GetLobbyInfoFromCreateLobbyRequest(CreateLobbyRequest request)
        {
            var lobbyInfo = new LobbyInfo
            {
                Id = Guid.NewGuid().ToString(),
            };
            var player = new Player
            {
                Ip = request.Ip,
                Port = request.Port,
                Name = request.PlayerName,
            };
            lobbyInfo.Players.Add(player);
            return lobbyInfo;
        }
    }
}

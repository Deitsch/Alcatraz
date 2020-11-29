using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Proto;
using System.Collections.Generic;
using System;
using RegistrationServer.Repositories;
using RegistrationServer.Spread;
using RegistrationServer.Listener;

namespace RegistrationServer
{

    public class LobbyService : Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> _logger;
        private readonly ISpreadConnection _spreadConn;
        private readonly MessageListener listener;
        private readonly LobbyRepository _lobbyRepository;

        public LobbyService(ILogger<LobbyService> logger, ISpreadConnection spreadConn, MessageListener listener , LobbyRepository lobbyRepository)
        {
            _logger = logger;
            _spreadConn = spreadConn;
            _lobbyRepository = lobbyRepository;
            this.listener = listener;
        }

        public override Task<GetLobbiesResponse> GetLobbies(GetLobbiesRequest request, ServerCallContext context)
        {
            var response = new GetLobbiesResponse();

            response.Lobbies.AddRange(_lobbyRepository.GetLobbies());
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            
            LobbyInfo lobbyInfo = getLobbyInfoFromCreateLobbyRequest(request);

            if (_spreadConn.IsPrimary)
            {
                _spreadConn.multicastLobbyInfoToReplicas(lobbyInfo);
                GetAcknFromReplicas();
                if (ackn)
                    _lobbyRepository.SaveLobby(lobbyInfo);
                else
                {
                    // throw exception
                }
            }
            else
            {
                _spreadConn.MulticastLobbyInfoToPrimary(lobbyInfo);
                lobbyInfo = _spreadConn.ReceiveLobbyInfoFromPrimary();
            }

            var response = new JoinLobbyResponse
            {
                Lobby = lobbyInfo
            };
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
            _spreadConn.SendMulticast(SpreadMulticastType.NewPlayerJoined, $"New Player: {request.PlayerName}");
            return Task.FromResult(response);
        }

        private LobbyInfo getLobbyInfoFromCreateLobbyRequest(CreateLobbyRequest request)
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

        private bool GetAcknFromReplicas(LobbyInfo lobbyInfo)
        {
         
        }
    }
}

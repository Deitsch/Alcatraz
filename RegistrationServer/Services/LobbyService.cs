using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Proto;
using System.Collections.Generic;
using System;

namespace RegistrationServer
{
    public class LobbyService : Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> _logger;
        private readonly ISpreadConn spreadConn;
        private readonly List<LobbyInfo> lobbies;

        public LobbyService(ILogger<LobbyService> logger, ISpreadConn spreadConn)
        {
            _logger = logger;
            this.spreadConn = spreadConn;
            lobbies = new List<LobbyInfo>();
        }

        public override Task<JoinLobbyResponse> CreateLobby(CreateLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
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
            lobbies.Add(lobbyInfo);
            response.Lobby = lobbyInfo;
            return Task.FromResult(response);
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var response = new JoinLobbyResponse();
            //var lobby = lobbies.Find(x => x.Id == request.LobbyId);
            //response.Players.Add(new Player { Name = "Hans" });
            //response.Players.Add(new Player { Name = "Fritz" });
            //response.Players.Add(new Player { Name = request.PlayerName });
            spreadConn.SendMessage($"New Player: {request.PlayerName}");
            return Task.FromResult(response);
        }
    }
}

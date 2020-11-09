using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Proto;
using System.Collections.Generic;

namespace RegistrationServer
{
    public class LobbyService : Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> _logger;
        private readonly ISpreadConn spreadConn;

        public LobbyService(ILogger<LobbyService> logger, ISpreadConn spreadConn)
        {
            _logger = logger;
            this.spreadConn = spreadConn;
        }

        public override Task<JoinLobbyResponse> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            var bla = new JoinLobbyResponse();
            bla.Players.Add(new Player { Name = "Hans" });
            bla.Players.Add(new Player { Name = "Fritz" });
            bla.Players.Add(new Player { Name = request.Name });
            spreadConn.SendMessage($"New Player: {request.Name}");
            return Task.FromResult(bla);
        }
    }
}

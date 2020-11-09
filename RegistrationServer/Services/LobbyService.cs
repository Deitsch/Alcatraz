using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RegistrationServer.Spread.Interface;
using RegistrationServer.Protos;

namespace RegistrationServer
{
    public class LobbyService : Lobby.LobbyBase
    {
        private readonly ILogger<LobbyService> _logger;
        public LobbyService(ILogger<LobbyService> logger)
        {
            _logger = logger;
        }

        public override Task<Player> JoinLobby(JoinLobbyRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Player
            {
                Name = "Hello " + request.Name
            });
        }
    }
}

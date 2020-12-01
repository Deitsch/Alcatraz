using RegistrationServer.Lobby.Proto;
using RegistrationServer.Repositories;
using System.Collections.Generic;

namespace RegistrationServer.Spread
{
    public class GetLobbiesOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public GetLobbiesOperation(LobbyRepository lobbyRepository)
        {
            this.lobbyRepository = lobbyRepository;
        }

        public List<LobbyInfo> Execute()
        {
            List<LobbyInfo> lobbies = lobbyRepository.FindAll();
            return lobbies;
        }
    }
}

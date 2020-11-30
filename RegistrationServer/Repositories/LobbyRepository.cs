using RegistrationServer.Lobby.Proto;
using System.Collections.Generic;

namespace RegistrationServer.Repositories
{
    public class LobbyRepository
    {
        private readonly List<LobbyInfo> lobbies;

        public LobbyRepository()
        {
            lobbies = new List<LobbyInfo>();
        }

        public List<LobbyInfo> GetLobbies()
        {
            return lobbies;
        }

        public void SaveLobby(LobbyInfo lobby)
        {
            lobbies.Add(lobby);
        }
    }
}

using RegistrationServer.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

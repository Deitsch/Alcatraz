using RegistrationServer.Lobby.Proto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistrationServer.Repositories
{
    public class LobbyRepository
    {
        private readonly List<LobbyInfo> lobbies;

        public LobbyRepository()
        {
            lobbies = new List<LobbyInfo>();
        }

        public LobbyInfo FindById(string lobbyId)
        {
            return lobbies.Single(lobby => lobby.Id == lobbyId);
        }

        public List<LobbyInfo> FindAll()
        {
            return lobbies;
        }

        public void Save(string lobbyId, NetworkPlayer player)
        {
            var lobby = new LobbyInfo
            {
                Id = lobbyId
            };
            lobby.Players.Add(player);

            lobbies.Add(lobby);
            Console.WriteLine("Saved Lobby with Id: " + lobbyId);
        }

        public void Delete(string lobbyId)
        {
            lobbies.Remove(FindById(lobbyId));
        }

        public void Delete(LobbyInfo lobby)
        {
            Console.WriteLine("Deleted Lobby with Id: " + lobby.Id);
            lobbies.Remove(lobby);
        }

        public void JoinLobby(string lobbyId, NetworkPlayer player)
        {
            Console.WriteLine($"Player: {player.Name} joined Lobby with Id: {lobbyId}");
            FindById(lobbyId).Players.Add(player);
        }

        public void LeaveLobby(string lobbyId, NetworkPlayer player)
        {
            var lobbyToLeave = FindById(lobbyId);
            lobbyToLeave.Players.Remove(player);

            Console.WriteLine($"Player: {player.Name} leaved Lobby with Id: {lobbyId}");

            if (lobbyToLeave.Players.Count == 0)
            {
                Delete(lobbyId);
            }
        }

        public void UpdateAll(List<LobbyInfo> lobbies)
        {
            this.lobbies.Clear();
            this.lobbies.AddRange(lobbies);
            Console.WriteLine("lobbies updated successfully");
        }
    }
}

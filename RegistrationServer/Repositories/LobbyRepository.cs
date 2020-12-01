using Grpc.Core;
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

        public void Save(string lobbyId, Player player)
        {
            var lobby = new LobbyInfo
            {
                Id = lobbyId
            };
            lobby.Players.Add(player);

            lobbies.Add(lobby);
            Console.WriteLine("Saved Lobby with Id: " + lobbyId);
        }

        public void JoinLobby(string lobbyId, Player player)
        {
            var lobbyToJoin = FindById(lobbyId);

            if (lobbyToJoin == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Lobby with id {lobbyId} not found"));
            }

            if (lobbyToJoin.Players.Any(p => p.Name == player.Name))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists,
                    $"A player with that name is already in lobby {lobbyId}"));
            }

            if (lobbyToJoin.Players.Count == 4)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition,
                    $"Lobby with id {lobbyId} is already full"));
            }

            lobbyToJoin.Players.Add(player);

            Console.WriteLine($"Player: {player.Name} joined Lobby with Id: {lobbyId}");
        }

        public void LeaveLobby(string lobbyId, Player player)
        {
            var lobbyToLeave = FindById(lobbyId);

            if (lobbyToLeave == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Lobby with id {lobbyId} not found"));
            }

            if (!lobbyToLeave.Players.Remove(player))
            {
                throw new RpcException(new Status(StatusCode.NotFound,
                    $"Player {player.Name} not found in Lobby {lobbyId}"));
            }

            Console.WriteLine($"Player: {player.Name} leaved Lobby with Id: {lobbyId}");

            if (lobbyToLeave.Players.Count == 0)
            {
                lobbies.Remove(lobbyToLeave);
                Console.WriteLine($"Deleted Lobby with Id: {lobbyId}");
            }
        }
    }
}

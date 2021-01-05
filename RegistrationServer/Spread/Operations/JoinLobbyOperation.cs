using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;

namespace RegistrationServer.Spread
{
    public class JoinLobbyOperation : PassiveReplicationOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public JoinLobbyOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository)
            : base(OperationType.JoinLobby, listener, spreadService)
        {
            this.lobbyRepository = lobbyRepository;
        }

        protected override void SpecificOperation(SpreadDto spreadDto)
        {
            lobbyRepository.JoinLobby(spreadDto.LobbyId, spreadDto.Player);
        }
    }
}

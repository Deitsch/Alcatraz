﻿using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;

namespace RegistrationServer.Spread
{
    public class LeaveLobbyOperation : PassiveReplicationOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public LeaveLobbyOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository)
            : base(OperationType.LeaveLobby, listener, spreadService)
        {
            this.lobbyRepository = lobbyRepository;
        }

        protected override void SpecificOperation(SpreadDto spreadDto)
        {
            lobbyRepository.LeaveLobby(spreadDto.LobbyId, spreadDto.Player);
        }
    }
}

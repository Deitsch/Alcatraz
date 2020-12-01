using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using RegistrationServer.utils;
using spread;
using System;

namespace RegistrationServer.Spread
{
    public class CreateLobbyOperation : PassiveReplicationOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public CreateLobbyOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository) 
            : base(OperationType.CreateLobby, listener, spreadService)
        {
            this.lobbyRepository = lobbyRepository;
        }

        protected override void SpecificOperation(SpreadMessage message)
        {
            try
            {
                lobbyRepository.Save(message.GetLobbyId(), message.GetPlayer());
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

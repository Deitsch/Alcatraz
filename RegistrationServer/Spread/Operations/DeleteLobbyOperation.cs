using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;

namespace RegistrationServer.Spread
{
    public class DeleteLobbyOperation : PassiveReplicationOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public DeleteLobbyOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository)
            : base(OperationType.DeleteLobby, listener, spreadService)
        {
            this.lobbyRepository = lobbyRepository;
        }

        protected override void SpecificOperation(SpreadDto spreadDto)
        {
            lobbyRepository.Delete(spreadDto.LobbyId);
        }
    }
}

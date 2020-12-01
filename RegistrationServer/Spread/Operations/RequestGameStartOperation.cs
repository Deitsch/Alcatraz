using RegistrationServer.Listener;
using RegistrationServer.Repositories;
using RegistrationServer.Spread.Enums;
using RegistrationServer.Spread.Interface;
using spread;

namespace RegistrationServer.Spread
{
    public class RequestGameStartOperation : PassiveReplicationOperation
    {
        private readonly LobbyRepository lobbyRepository;

        public RequestGameStartOperation(MessageListener listener, ISpreadService spreadService, LobbyRepository lobbyRepository)
            : base(OperationType.RequestGameStart, listener, spreadService)
        {
            this.lobbyRepository = lobbyRepository;
        }

        protected override void SpecificOperation(SpreadMessage message)
        {
            // ToDo
        }
    }
}

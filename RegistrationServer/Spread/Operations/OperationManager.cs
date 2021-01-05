namespace RegistrationServer.Spread.Operations
{
    class OperationManager : IOperationManager
    {
        private readonly CreateLobbyOperation createLobbyOperation;
        private readonly JoinLobbyOperation joinLobbyOperation;
        private readonly LeaveLobbyOperation leaveLobbyOperation;
        private readonly DeleteLobbyOperation deleteLobbyOperation;
        private readonly UpdateIpAddressesOperation updateIpAddressesOperation;

        public OperationManager(
            CreateLobbyOperation createLobbyOperation,
            JoinLobbyOperation joinLobbyOperation,
            LeaveLobbyOperation leaveLobbyOperation,
            DeleteLobbyOperation deleteLobbyOperation,
            UpdateIpAddressesOperation updateIpAddressesOperation)
        {
            this.createLobbyOperation = createLobbyOperation;
            this.joinLobbyOperation = joinLobbyOperation;
            this.leaveLobbyOperation = leaveLobbyOperation;
            this.deleteLobbyOperation = deleteLobbyOperation;
            this.updateIpAddressesOperation = updateIpAddressesOperation;
        }

        public void AddOperationListeners()
        {
            createLobbyOperation.AddListener();
            joinLobbyOperation.AddListener();
            leaveLobbyOperation.AddListener();
            deleteLobbyOperation.AddListener();
            updateIpAddressesOperation.AddListener();
            updateIpAddressesOperation.AddStartListener();
        }
    }
}

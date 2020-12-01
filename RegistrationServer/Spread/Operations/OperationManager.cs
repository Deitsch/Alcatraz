namespace RegistrationServer.Spread.Operations
{
    class OperationManager : IOperationManager
    {
        private readonly CreateLobbyOperation createLobbyOperation;
        private readonly JoinLobbyOperation joinLobbyOperation;
        private readonly LeaveLobbyOperation leaveLobbyOperation;
        private readonly RequestGameStartOperation requestGameStartOperation;

        public OperationManager(
            CreateLobbyOperation createLobbyOperation,
            JoinLobbyOperation joinLobbyOperation,
            LeaveLobbyOperation leaveLobbyOperation,
            RequestGameStartOperation requestGameStartOperation)
        {
            this.createLobbyOperation = createLobbyOperation;
            this.joinLobbyOperation = joinLobbyOperation;
            this.leaveLobbyOperation = leaveLobbyOperation;
            this.requestGameStartOperation = requestGameStartOperation;
        }

        public void AddOperationListeners()
        {
            createLobbyOperation.AddListener();
            joinLobbyOperation.AddListener();
            leaveLobbyOperation.AddListener();
            requestGameStartOperation.AddListener();
        }
    }
}

namespace RegistrationServer.Spread.Operations
{
    class OperationManager : IOperationManager
    {
        private readonly CreateLobbyOperation createLobbyOperation;

        public OperationManager(CreateLobbyOperation createLobbyOperation)
        {
            this.createLobbyOperation = createLobbyOperation;
        }

        public void AddOperationListeners()
        {
            createLobbyOperation.AddListener();
        }
    }
}

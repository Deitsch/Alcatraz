namespace RegistrationServer.Spread
{
    public enum MulticastType
    {
        // type 0 = unset
        NewPrimary = 1,
        CreateLobbyToPrimary,
        CreateLobbyToReplicas,
        CreateLobbyAcknToPrimary,
        CreateLobbyToOriginalSenderSuccessfully,
        CreateLobbyToOriginalSenderNotSuccessfully,
        NewPlayerJoined
    }
}

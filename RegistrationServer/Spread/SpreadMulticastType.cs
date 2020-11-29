namespace RegistrationServer.Spread
{
    public enum SpreadMulticastType
    {
        // type 0 = unset
        NewPrimary = 1,
        CreateLobbyToPrimary,
        CreateLobbyToReplicas,
        ReceiveLobbyFromPrimary,
        CreateLobbyAckn,
        NewPlayerJoined
    }
}

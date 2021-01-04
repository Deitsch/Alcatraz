namespace RegistrationServer.Spread
{
    public enum MulticastType
    {
        NewPrimary = 1,
        ToPrimary,
        ToReplicas,
        AcknToPrimary,
        ToOriginalSenderSuccessfully,
        ToOriginalSenderNotSuccessfully,
        UpdateDb
    }
}

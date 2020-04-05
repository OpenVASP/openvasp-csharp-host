namespace OpenVASP.Messaging.Messages
{
    public enum MessageType
    {
        SessionRequest = 110,

        SessionReply = 150,

        TransferRequest = 210,

        TransferReply = 250,

        TransferDispatch = 310,

        TransferConfirmation = 350,

        Termination = 910,
    }
}

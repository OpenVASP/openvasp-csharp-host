namespace OpenVASP.Host.Core.Models
{
    public enum TransactionStatus
    {
        Created,
        SessionRequested,
        SessionConfirmed,
        SessionDeclined,
        TransferRequested,
        TransferForbidden,
        TransferAllowed,
        TransferDispatched,
        TransferConfirmed
    }
}
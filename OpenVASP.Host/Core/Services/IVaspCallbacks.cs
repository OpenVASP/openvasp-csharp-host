using System.Threading.Tasks;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Host.Core.Services
{
    public interface IVaspCallbacks
    {
        Task SessionRequestMessageReceivedAsync(string sessionId, SessionRequestMessage message);
        Task SessionReplyMessageReceivedAsync(string sessionId, SessionReplyMessage message);
        Task TransferReplyMessageReceivedAsync(string sessionId, TransferReplyMessage message);
        Task TransferConfirmationMessageReceivedAsync(string sessionId, TransferConfirmationMessage message);
        Task TransferRequestMessageReceivedAsync(string sessionId, TransferRequestMessage message);
        Task TransferDispatchMessageReceivedAsync(string sessionId, TransferDispatchMessage message);
    }
}
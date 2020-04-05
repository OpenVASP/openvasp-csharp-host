using System.Threading;
using System.Threading.Tasks;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Messaging.MessagingEngine
{
    public abstract class MessageHandlerBase
    {
        public abstract Task HandleMessageAsync(MessageBase message, CancellationToken cancellationToken);
    }
}
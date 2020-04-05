using System;
using System.Threading;
using System.Threading.Tasks;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Messaging.MessagingEngine
{
    public class GenericMessageHandler<TMessage> : MessageHandlerBase
    {
        private Func<TMessage, CancellationToken, Task> messageProcessFunc;

        public GenericMessageHandler(Func<TMessage, CancellationToken, Task> messageProcessFunc)
        {
            this.messageProcessFunc = messageProcessFunc;
        }

        public override async Task HandleMessageAsync(MessageBase message, CancellationToken cancellationToken)
        {
            if (message is TMessage messageForProcessing)
            {
                await messageProcessFunc(messageForProcessing, cancellationToken);
            }
        }
    }
}
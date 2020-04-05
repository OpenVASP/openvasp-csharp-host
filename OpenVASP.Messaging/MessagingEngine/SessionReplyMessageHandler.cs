using System;
using System.Threading;
using System.Threading.Tasks;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Messaging.MessagingEngine
{
    public class SessionRequestMessageHandler : GenericMessageHandler<SessionRequestMessage>
    {
        public SessionRequestMessageHandler(Func<SessionRequestMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class SessionReplyMessageHandler : GenericMessageHandler<SessionReplyMessage>
    {
        public SessionReplyMessageHandler(Func<SessionReplyMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class TransferRequestMessageHandler : GenericMessageHandler<TransferRequestMessage>
    {
        public TransferRequestMessageHandler(Func<TransferRequestMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class TransferReplyMessageHandler : GenericMessageHandler<TransferReplyMessage>
    {
        public TransferReplyMessageHandler(Func<TransferReplyMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class TransferDispatchMessageHandler : GenericMessageHandler<TransferDispatchMessage>
    {
        public TransferDispatchMessageHandler(Func<TransferDispatchMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class TransferConfirmationMessageHandler : GenericMessageHandler<TransferConfirmationMessage>
    {
        public TransferConfirmationMessageHandler(Func<TransferConfirmationMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }

    public class TerminationMessageHandler : GenericMessageHandler<TerminationMessage>
    {
        public TerminationMessageHandler(Func<TerminationMessage, CancellationToken, Task> processFunc) :
            base(processFunc)
        {
        }
    }
}
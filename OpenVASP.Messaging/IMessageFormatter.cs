using OpenVASP.Messaging.Messages;

namespace OpenVASP.Messaging
{
    public interface IMessageFormatter
    {
        string GetPayload(MessageBase sessionRequestMessage);

        MessageBase Deserialize(string payload);
    }
}
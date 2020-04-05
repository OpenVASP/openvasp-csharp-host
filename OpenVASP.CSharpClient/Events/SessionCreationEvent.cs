namespace OpenVASP.CSharpClient.Events
{
    public class SessionCreatedEvent
    {
        public SessionCreatedEvent(string sessionId)
        {
            this.SessionId = sessionId;
        }
        public string SessionId { get;}
    }
}
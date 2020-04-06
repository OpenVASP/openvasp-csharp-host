namespace OpenVASP.CSharpClient.Events
{
    public class SessionTerminationEvent
    {
        public SessionTerminationEvent(string sessionId)
        {
            this.SessionId = sessionId;
        }
        public string SessionId { get;}
    }
}
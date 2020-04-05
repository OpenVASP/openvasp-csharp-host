namespace OpenVASP.CSharpClient.Interfaces
{
    public interface ISignService
    {
        string SignPayload(string payload, string privateKey);

        bool VerifySign(string payload, string sign, string pubKey);
    }
}
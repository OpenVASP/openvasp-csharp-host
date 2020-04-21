namespace OpenVASP.CSharpClient.Utils
{
    public static class Extensions
    {
        public static string ToStandardHex(this string s)
        {
            return s.StartsWith("0x04") ? s : $"0x04{s}";
        }
        
        public static string ToOpenVaspHex(this string s)
        {
            return s.StartsWith("0x04") ? s.Substring(4) : s;
        }
    }
}
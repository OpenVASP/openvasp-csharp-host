namespace OpenVASP.Messaging.Messages.Entities
{
    public class Beneficiary
    {
        public Beneficiary(string name, string vaan)
        {
            Name = name ?? "";
            VAAN = vaan;
        }

        public string Name { get; set; }

        public string VAAN { get; set; }
    }
}
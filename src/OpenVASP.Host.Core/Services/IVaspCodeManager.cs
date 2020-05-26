using System.Collections.Generic;

namespace OpenVASP.Host.Core.Services
{
    public interface IVaspCodeManager
    {
        bool IsAutoConfirmedVaspCode(string vaspCode);

        List<string> GetAutoConfirmedVaspCodes();
    }
}

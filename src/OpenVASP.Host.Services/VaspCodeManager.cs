using System.Collections.Generic;
using System.Linq;
using OpenVASP.Host.Core.Services;

namespace OpenVASP.Host.Services
{
    public class VaspCodeManager : IVaspCodeManager
    {
        private readonly HashSet<string> _vaspCodes = new HashSet<string>();

        public VaspCodeManager(IEnumerable<string> vaspCodes)
        {
            if (vaspCodes != null)
                foreach (var vaspCode in vaspCodes)
                {
                    _vaspCodes.Add(vaspCode.ToLower());
                }
        }

        public List<string> GetAutoConfirmedVaspCodes()
        {
            return _vaspCodes.ToList();
        }

        public bool IsAutoConfirmedVaspCode(string vaspCode)
        {
            return _vaspCodes.Contains(vaspCode.ToLower());
        }
    }
}

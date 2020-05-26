using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Host.Core.Services;

namespace OpenVASP.Host.Controllers
{
    [Route("api/vaspCodes")]
    public class VaspCodesController : Controller
    {
        private readonly IVaspCodeManager _vaspCodeManager;

        public VaspCodesController(IVaspCodeManager vaspCodeManager)
        {
            _vaspCodeManager = vaspCodeManager;
        }

        /// <summary>
        /// Get a list of vasp codes that are autoconfirmed.
        /// </summary>
        /// <returns>A list of autoconfirmed vasp codes.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAutoConfirmedVaspCodesAsync()
        {
            var vaspCodes = _vaspCodeManager.GetAutoConfirmedVaspCodes();

            return Ok(vaspCodes);
        }
    }
}

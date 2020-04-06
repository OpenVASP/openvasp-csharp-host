using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenVASP.Messaging.Messages.Entities;
using SessionDemo.Models;
using SessionDemo.Services;

namespace SessionDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly VaspSessionService _vaspSessionService;

        public HomeController(VaspSessionService vaspSessionService)
        {
            _vaspSessionService = vaspSessionService;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Index(CreateTransferModel transfer)
        {
            _vaspSessionService.CreateSessionRequest(
                transfer.BeneficiaryName,
                transfer.OriginatorName,
                new PostalAddress(
                    transfer.PostalAddressStreetName,
                    transfer.PostalAddressBuildingNumber,
                    transfer.PostalAddressAddressLine,
                    transfer.PostalAddressPostCode,
                    transfer.PostalAddressTownName,
                    Country.List.Single(x => x.Value.Name == transfer.PostalAddressCountry).Value),
                new PlaceOfBirth(transfer.DateOfBirth, transfer.PostalAddressTownName, Country.List.Single(x => x.Value.Name == transfer.PostalAddressCountry).Value), 
                transfer.AssetType,
                transfer.TransactionAmount);
 
            ModelState.Clear();
            
            return View();
        }

        public IActionResult PendingTransactions()
        {
            return View(_vaspSessionService
                .VaspSessions
                .Where(x => x.Value.State == VaspSessionState.WaitingForTransferRequestApproval )
                .ToDictionary(pair => pair.Key, pair => pair.Value.PendingTransferRequest)
                .ToList());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Approve(string id)
        {
            _vaspSessionService.ReplyToTransferRequest(id, true);

            return RedirectToAction("PendingTransactions");
        }

        public async Task<IActionResult> Cancel(string id)
        {
            _vaspSessionService.ReplyToTransferRequest(id, false);

            return RedirectToAction("PendingTransactions");
        }

        public IActionResult DispatchedTransactions()
        {
            return View(
                _vaspSessionService
                    .VaspSessions
                    .Where(x => x.Value.StatesHistory.Any(y => y.Item1 == VaspSessionState.TransferDispatched))
                    .Select(x => (x.Value.PendingTransferRequest, x.Key))
                    .ToList());
        }
    }
}

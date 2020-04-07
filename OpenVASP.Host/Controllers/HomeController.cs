using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Models.Response;
using OpenVASP.Host.Services;
using OpenVASP.Messaging.Messages.Entities;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Controllers
{
    public class HomeController : Controller
    {
        private readonly TransactionsManager _transactionsManager;

        public HomeController(TransactionsManager transactionsManager)
        {
            _transactionsManager = transactionsManager;
        }

        public async Task<IActionResult> Index()
        {
            var model = new MainPageModel
            {
                Countries = Country.List.Values.Select(x => x.Name).ToList(),
                Assets = new List<string> {"BTC", "ETH"},
                IncomingTransactions = (await _transactionsManager.GetIncomingTransactionsAsync())
                    .Select(x => new SimplifiedTransactionModel
                    {
                        Id = x.Id,
                        CreationDateTime = x.CreationDateTime,
                        Status = x.Status,
                        StatusStringified = Stringify(x.Status)
                    })
                    .ToList(),
                OutgoingTransactions = (await _transactionsManager.GetOutgoingTransactionsAsync())
                    .Select(x => new SimplifiedTransactionModel {
                        Id = x.Id,
                        CreationDateTime = x.CreationDateTime,
                        Status = x.Status,
                        StatusStringified = Stringify(x.Status)})
                    .ToList()
            };
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Transaction([FromRoute] string id)
        {
            var incomingTransactions = await _transactionsManager.GetIncomingTransactionsAsync();
            var outgoingTransactions = await _transactionsManager.GetOutgoingTransactionsAsync();

            Transaction transaction;
            TransactionDetailsModel model = new TransactionDetailsModel();

            if (incomingTransactions.Any(x => x.Id == id))
            {
                transaction = incomingTransactions.Single(x => x.Id == id);
                model.TransactionType = "Incoming Transaction";
            } else if (outgoingTransactions.Any(x => x.Id == id))
            {
                transaction = outgoingTransactions.Single(x => x.Id == id);
                model.TransactionType = "Outgoing Transaction";
            }
            else
            {
                return View(null);
            }

            model.CreationDateTime = transaction.CreationDateTime;
            model.Amount = transaction.Amount;
            model.Asset = transaction.Asset.ToString();
            model.OriginatorVaan = transaction.OriginatorVaan;
            model.OriginatorFullName = transaction.OriginatorFullName;
            model.OriginatorPostalAddressBuilding = transaction.OriginatorPostalAddress.Building;
            model.OriginatorPostalAddressCountry = transaction.OriginatorPostalAddress.Country.Name;
            model.OriginatorPostalAddressStreet = transaction.OriginatorPostalAddress.Street;
            model.OriginatorPostalAddressTown = transaction.OriginatorPostalAddress.Town;
            model.OriginatorPostalAddressAddressLine = transaction.OriginatorPostalAddress.AddressLine;
            model.OriginatorPostalAddressPostCode = transaction.OriginatorPostalAddress.PostCode;
            model.StatusStringified = Stringify(transaction.Status);
            model.SessionId = transaction.SessionId;
            model.Id = transaction.Id;
            model.BeneficiaryVaan = transaction.BeneficiaryVaan;
            model.BeneficiaryFullName = transaction.BeneficiaryFullName;
            model.SendingAddress = transaction.SendingAddress;
            model.DestinationAddress = transaction.DestinationAddress;
            model.TransactionHash = transaction.TransactionHash;
            model.OriginatorPlaceOfBirthCountry = transaction.OriginatorPlaceOfBirth.Country.Name;
            model.OriginatorPlaceOfBirthDate = transaction.OriginatorPlaceOfBirth.Date;
            model.OriginatorPlaceOfBirthTown = transaction.OriginatorPlaceOfBirth.Town;
            
            return View(model);
        }

        private string Stringify(TransactionStatus status)
        {
            return status switch
            {
                TransactionStatus.Created => "Created",
                TransactionStatus.SessionRequested => "Sessions Requested",
                TransactionStatus.SessionConfirmed => "Session Confirmed",
                TransactionStatus.TransferRequested => "Transfer Requested",
                TransactionStatus.TransferForbidden => "Transfer Forbidden",
                TransactionStatus.TransferAllowed => "Transfer Allowed",
                TransactionStatus.TransferDispatched => "Transfer Dispatched",
                TransactionStatus.TransferConfirmed => "Transfer Confirmed",
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
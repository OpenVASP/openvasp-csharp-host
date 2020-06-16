using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Core.Services;
using OpenVASP.Host.Models.Request;
using OpenVASP.Host.Models.Response;
using OpenVASP.Messaging.Messages.Entities;
using PlaceOfBirth = OpenVASP.Host.Core.Models.PlaceOfBirth;
using PostalAddress = OpenVASP.Host.Core.Models.PostalAddress;

namespace OpenVASP.Host.Controllers
{
    [Route("api/outgoingTransactions")]
    public class OutgoingTransactionsController : Controller
    {
        private readonly ITransactionDataService _transactionDataService;
        private readonly ITransactionsManager _transactionsManager;
        private readonly IMapper _mapper;

        public OutgoingTransactionsController(
            ITransactionDataService transactionDataService,
            ITransactionsManager transactionsManager,
            IMapper mapper)
        {
            _transactionDataService = transactionDataService;
            _transactionsManager = transactionsManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all outgoing transactions for the given host.
        /// </summary>
        /// <returns>A list of outgoing transactions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TransactionDetailsModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOutgoingTransactionsAsync()
        {
            var txs = await _transactionsManager.GetOutgoingTransactionsAsync();

            return Ok(_mapper.Map<List<TransactionDetailsModel>>(txs));
        }

        /// <summary>
        /// Get a specific outgoing transaction for the given host.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>A requested transaction.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetOutgoingTransactionAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ValidationProblem(
                    new ValidationProblemDetails(
                        new Dictionary<string, string[]>
                        {
                            { nameof(id), new [] { $"{nameof(id)} is required" } }
                        }));

            var transaction = await _transactionsManager.GetAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }

        /// <summary>
        /// Create a transaction (send SessionRequest) and make a transfer request (send TransferRequest).
        /// </summary>
        /// <param name="model">The details for the to-be-created transaction.</param>
        /// <returns>The created transaction.</returns>
        /// <exception cref="InvalidOperationException">In case an invalid Asset was provided.</exception>
        [HttpPost("create")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateOutgoingTransactionRequestModel model)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var asset = model.Asset switch
            {
                "ETH" => VirtualAssetType.ETH,
                "BTC" => VirtualAssetType.BTC,
                _ => throw new NotSupportedException($"Asset {model.Asset} not recognized."),
            };
            PlaceOfBirth placeOfBirth = null;
            if (model.OriginatorPlaceOfBirth != null)
            {
                var originatorPlaceOfBirthCountry =
                Country
                    .List
                    .Single(x => x.Value.TwoLetterCode == model.OriginatorPlaceOfBirth.CountryIso2Code)
                    .Value;
                placeOfBirth = new PlaceOfBirth
                {
                    Country = originatorPlaceOfBirthCountry,
                    Date = model.OriginatorPlaceOfBirth.Date,
                    Town = model.OriginatorPlaceOfBirth.Town
                };
            }

            var originatorPostalAddressCountry =
                Country
                    .List
                    .Single(x => x.Value.TwoLetterCode == model.OriginatorPostalAddress.CountryIso2Code)
                    .Value;

            var transaction = _transactionDataService.GenerateTransactionData(
                model.OriginatorFullName,
                model.OriginatorVaan,
                placeOfBirth,
                new PostalAddress
                {
                    Country = originatorPostalAddressCountry,
                    AddressLine = model.OriginatorPostalAddress.AddressLine,
                    Building = model.OriginatorPostalAddress.Building,
                    PostCode = model.OriginatorPostalAddress.PostCode,
                    Street = model.OriginatorPostalAddress.Street,
                    Town = model.OriginatorPostalAddress.Town
                },
                model.BeneficiaryFullName,
                model.BeneficiaryVaan,
                asset,
                model.Amount,
                model.OriginatorNaturalPersonIds?
                    .Select(x => new NaturalPersonId(x.Id, x.Type, Country.List[x.CountryIso2Code], x.NonStateIssuer))
                    .ToArray(),
                model.OriginatorJuridicalPersonIds?
                    .Select(x => new JuridicalPersonId(x.Id, x.Type, Country.List[x.CountryIso2Code], x.NonStateIssuer))
                    .ToArray(),
                model.OriginatorBic,
                TransactionType.Outgoing);

            transaction = await _transactionsManager.RegisterOutgoingTransactionAsync(
                transaction,
                _transactionDataService.CreateVirtualAssetsAccountNumber(model.BeneficiaryVaan));

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }

        /// <summary>
        /// Send a TransferDispatch message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="sendingAddress">The (blockchain) sending address.</param>
        /// <param name="transactionHash">The (blockchain) transaction hash.</param>
        /// <param name="transactionDateTime">The (blockchain) transaction date and time.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferDispatch")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendTransferDispatchAsync(
            [FromRoute] string id,
            [FromQuery] string sendingAddress,
            [FromQuery] string transactionHash,
            [FromQuery] DateTime transactionDateTime)
        {
            var validationErrorsDict = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(id))
                validationErrorsDict.Add(nameof(id), new[] { $"{nameof(id)} is required" });

            if (string.IsNullOrWhiteSpace(sendingAddress))
                validationErrorsDict.Add(nameof(sendingAddress), new[] { $"{nameof(sendingAddress)} is required" });

            if (string.IsNullOrWhiteSpace(transactionHash))
                validationErrorsDict.Add(nameof(transactionHash), new[] { $"{nameof(transactionHash)} is required" });

            if (validationErrorsDict.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(validationErrorsDict));

            try
            {
                await _transactionsManager.SendTransferDispatchAsync(id, sendingAddress, transactionHash, transactionDateTime);
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }

            var transaction = await _transactionsManager.GetAsync(id);

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }
    }
}
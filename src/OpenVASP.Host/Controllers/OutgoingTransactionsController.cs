using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.Host.Models.Request;
using OpenVASP.Host.Services;
using PlaceOfBirth = OpenVASP.Host.Core.Models.PlaceOfBirth;
using PostalAddress = OpenVASP.Host.Core.Models.PostalAddress;

namespace OpenVASP.Host.Controllers
{
    [Route("api/outgoingTransactions")]
    public class OutgoingTransactionsController : Controller
    {
        private readonly TransactionDataProcessor _transactionDataGenerator;
        private readonly TransactionsManager _transactionsManager;

        public OutgoingTransactionsController(
            TransactionDataProcessor transactionDataGenerator,
            TransactionsManager transactionsManager)
        {
            _transactionDataGenerator = transactionDataGenerator;
            _transactionsManager = transactionsManager;
        }

        /// <summary>
        /// Get all outgoing transactions for the given host.
        /// </summary>
        /// <returns>A list of outgoing transactions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetOutgoingTransactionsAsync()
        {
            return Ok(await _transactionsManager.GetOutgoingTransactionsAsync());
        }

        /// <summary>
        /// Get a specific outgoing transaction for the given host.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>A requested transaction.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOutgoingTransactionAsync([FromRoute] string id)
        {
            var transaction = (await _transactionsManager.GetOutgoingTransactionsAsync())
                .SingleOrDefault(x => x.Id == id);

            return Ok(transaction);
        }

        /// <summary>
        /// Create a transaction (send SessionRequest) and make a transfer request (send TransferRequest).
        /// </summary>
        /// <param name="model">The details for the to-be-created transaction.</param>
        /// <returns>The created transaction.</returns>
        /// <exception cref="InvalidOperationException">In case an invalid Asset was provided.</exception>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateOutgoingTransactionRequestModel model)
        {
            #region Validations
            
            var originatorPlaceOfBirthCountry =
                Country
                    .List
                    .Single(x => x.Value.Name == model.OriginatorPlaceOfBirth.Country)
                    .Value;
            
            var originatorPostalAddressCountry =
                Country
                    .List
                    .Single(x => x.Value.Name == model.OriginatorPostalAddress.Country)
                    .Value;

            if (model.Asset != "ETH" && model.Asset != "BTC")
            {
                throw new ArgumentException("Asset not recognized.");
            }

            var asset = model.Asset == "ETH" ? VirtualAssetType.ETH : VirtualAssetType.BTC;
            
            if (model.OriginatorPlaceOfBirth == null && model.OriginatorNaturalPersonIds == null &&
                model.OriginatorJuridicalPersonIds == null && model.OriginatorBic == null)
            {
                throw new ArgumentException("Originator needs to be either a bank, a natural person or a juridical person.");
            }

            if ((model.OriginatorPlaceOfBirth != null || model.OriginatorNaturalPersonIds != null) &&
                (model.OriginatorJuridicalPersonIds != null || model.OriginatorBic != null))
            {
                throw new ArgumentException("Originator can't be several types of entities at once.");
            }
            
            if (model.OriginatorJuridicalPersonIds != null && ( model.OriginatorNaturalPersonIds != null ||
                model.OriginatorPlaceOfBirth != null || model.OriginatorBic != null))
            {
                throw new ArgumentException("Originator can't be several types of entities at once.");
            }
            
            if (model.OriginatorBic != null && ( model.OriginatorNaturalPersonIds != null ||
                model.OriginatorPlaceOfBirth != null || model.OriginatorJuridicalPersonIds != null))
            {
                throw new ArgumentException("Originator can't be several types of entities at once.");
            }

            if (model.OriginatorJuridicalPersonIds != null && model.OriginatorJuridicalPersonIds.Any(
                x => !Country.List.ContainsKey(x.CountryCode)))
            {
                throw new ArgumentException("Invalid country code for Juridical Person Id.");
            }
            
            if (model.OriginatorNaturalPersonIds != null && model.OriginatorNaturalPersonIds.Any(
                x => !Country.List.ContainsKey(x.CountryCode)))
            {
                throw new ArgumentException("Invalid country code for Natural Person Id.");
            }

            #endregion Validations

            var (transaction, originator) = _transactionDataGenerator.GenerateTransactionData(
                model.OriginatorFullName,
                model.OriginatorVaan,
                new PlaceOfBirth
                {
                    Country = originatorPlaceOfBirthCountry,
                    Date = model.OriginatorPlaceOfBirth.Date,
                    Town = model.OriginatorPlaceOfBirth.Town
                },
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
                    .Select(x => new NaturalPersonId(x.Id, x.Type, Country.List[x.CountryCode], x.NonStateIssuer))
                    .ToArray(),
                model.OriginatorJuridicalPersonIds?
                    .Select(x => new JuridicalPersonId(x.Id, x.Type, Country.List[x.CountryCode], x.NonStateIssuer))
                    .ToArray(),
                model.OriginatorBic);

            transaction = await _transactionsManager.RegisterOutgoingTransactionAsync(
                transaction,
                originator,
                _transactionDataGenerator.CreateVirtualAssetsAccountNumber(model.BeneficiaryVaan));

            return Ok(transaction);
        }

        /// <summary>
        /// Send a TransferDispatch message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="sendingAddress">The (blockchain) sending address.</param>
        /// <param name="transactionHash">The (blockchain) transaction hash.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferDispatch")]
        public async Task<IActionResult> SendTransferDispatchAsync(
            [FromRoute] string id,
            [FromQuery] string sendingAddress,
            [FromQuery] string transactionHash
            )
        {
            await _transactionsManager.SendTransferDispatchAsync(id, sendingAddress, transactionHash);

            return await GetOutgoingTransactionAsync(id);
        }
    }
}
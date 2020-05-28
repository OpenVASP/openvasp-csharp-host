using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Host.Core.Services;
using OpenVASP.Host.Models.Response;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Host.Controllers
{
    [Route("api/incomingTransactions")]
    public class IncomingTransactionsController : Controller
    {
        private readonly ITransactionsManager _transactionsManager;
        private readonly IMapper _mapper;

        public IncomingTransactionsController(
            ITransactionsManager transactionsManager,
            IMapper mapper)
        {
            _transactionsManager = transactionsManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all incoming transactions for the given host.
        /// </summary>
        /// <returns>A list of incoming transactions.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<TransactionDetailsModel>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIncomingTransactionsAsync()
        {
            var txs = await _transactionsManager.GetIncomingTransactionsAsync();

            return Ok(_mapper.Map<List<TransactionDetailsModel>>(txs));
        }

        /// <summary>
        /// Get a specific incoming transaction for the given host.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>A requested transaction.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetIncomingTransactionAsync([FromRoute] string id)
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
        /// Send a TransferReply message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="code">Session reply message code.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/sessionReply")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendSessionReplyAsync(
            [FromRoute] string id,
            [FromQuery] SessionReplyMessage.SessionReplyMessageCode code)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ValidationProblem(
                    new ValidationProblemDetails(
                        new Dictionary<string, string[]>
                        {
                            { nameof(id), new [] { $"{nameof(id)} is required" } }
                        }));
            try
            {
                await _transactionsManager.SendSessionReplyAsync(id, code);
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }

            var transaction = await _transactionsManager.GetAsync(id);

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }

        /// <summary>
        /// Send a TransferReply message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="code">Transfer reply message code.</param>
        /// <param name="destinationAddress">The (blockchain) destination address of the beneficiary.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferReply")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendTransferReplyAsync(
            [FromRoute] string id,
            [FromQuery] TransferReplyMessage.TransferReplyMessageCode code,
            [FromQuery] string destinationAddress)
        {
            var validationErrorsDict = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(id))
                validationErrorsDict.Add(nameof(id), new[] { $"{nameof(id)} is required" });

            if (string.IsNullOrWhiteSpace(destinationAddress))
                validationErrorsDict.Add(nameof(destinationAddress), new[] { $"{nameof(destinationAddress)} is required" });

            if (validationErrorsDict.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(validationErrorsDict));

            try
            {
                await _transactionsManager.SendTransferReplyAsync(
                id,
                destinationAddress,
                code);
            }
            catch (NullReferenceException)
            {
                return NotFound();
            }            

            var transaction = await _transactionsManager.GetAsync(id);

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }

        /// <summary>
        /// Send a TransferConfirm message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferConfirm")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendTransferConfirmAsync([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ValidationProblem(
                    new ValidationProblemDetails(
                        new Dictionary<string, string[]>
                        {
                            { nameof(id), new [] { $"{nameof(id)} is required" } }
                        }));

            try
            {
                await _transactionsManager.SendTransferConfirmAsync(id);
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
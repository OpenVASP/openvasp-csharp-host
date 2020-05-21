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
            var transaction = (await _transactionsManager.GetIncomingTransactionsAsync())
                .SingleOrDefault(x => x.Id == id);

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
            await _transactionsManager.SendSessionReplyAsync(id, code);

            var transaction = await GetIncomingTransactionAsync(id);

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
            await _transactionsManager.SendTransferReplyAsync(
                id,
                destinationAddress,
                code);

            var transaction = await GetIncomingTransactionAsync(id);

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }

        /// <summary>
        /// Send a TransferConfirm message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferConfirm")]
        [ProducesResponseType(typeof(TransactionDetailsModel), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> SendTransferConfirmAsync(
            [FromRoute] string id)
        {
            await _transactionsManager.SendTransferConfirmAsync(id);

            var transaction = await GetIncomingTransactionAsync(id);

            return Ok(_mapper.Map<TransactionDetailsModel>(transaction));
        }
    }
}
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenVASP.Host.Services;
using OpenVASP.Messaging.Messages;

namespace OpenVASP.Host.Controllers
{
    [Route("api/incomingTransactions")]
    public class IncomingTransactionsController : Controller
    {
        private readonly TransactionsManager _transactionsManager;

        public IncomingTransactionsController(TransactionsManager transactionsManager)
        {
            _transactionsManager = transactionsManager;
        }

        /// <summary>
        /// Get all incoming transactions for the given host.
        /// </summary>
        /// <returns>A list of incoming transactions.</returns>
        [HttpGet]
        public async Task<IActionResult> GetIncomingTransactionsAsync()
        {
            return Ok(await _transactionsManager.GetIncomingTransactionsAsync());
        }

        /// <summary>
        /// Get a specific incoming transaction for the given host.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>A requested transaction.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetIncomingTransactionAsync([FromRoute] string id)
        {
            var transaction = (await _transactionsManager.GetIncomingTransactionsAsync())
                .SingleOrDefault(x => x.Id == id);

            return Ok(transaction);
        }

        /// <summary>
        /// Send a TransferReply message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="code">Session reply message code.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/sessionReply")]
        public async Task<IActionResult> SendSessionReplyAsync(
            [FromRoute] string id,
            [FromQuery] SessionReplyMessage.SessionReplyMessageCode code)
        {
            await _transactionsManager.SendSessionReplyAsync(id, code);

            return await GetIncomingTransactionAsync(id);
        }

        /// <summary>
        /// Send a TransferReply message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <param name="code">Transfer reply message code.</param>
        /// <param name="destinationAddress">The (blockchain) destination address of the beneficiary.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferReply")]
        public async Task<IActionResult> SendTransferReplyAsync(
            [FromRoute] string id,
            [FromQuery] TransferReplyMessage.TransferReplyMessageCode code,
            [FromQuery] string destinationAddress)
        {
            await _transactionsManager.SendTransferReplyAsync(
                id,
                destinationAddress,
                code);

            return await GetIncomingTransactionAsync(id);
        }

        /// <summary>
        /// Send a TransferConfirm message for the given transaction.
        /// </summary>
        /// <param name="id">The Id of the transaction.</param>
        /// <returns>The updated transaction.</returns>
        [HttpPut("{id}/transferConfirm")]
        public async Task<IActionResult> SendTransferConfirmAsync(
            [FromRoute] string id)
        {
            await _transactionsManager.SendTransferConfirmAsync(id);

            return await GetIncomingTransactionAsync(id);
        }
    }
}
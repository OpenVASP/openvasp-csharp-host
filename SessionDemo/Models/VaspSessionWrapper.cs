using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenVASP.CSharpClient;
using OpenVASP.CSharpClient.Interfaces;
using OpenVASP.CSharpClient.Sessions;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using SessionDemo.Services;

namespace SessionDemo.Models
{
    public class VaspSessionWrapper
    {
        public string Id { get; }

        public VaspSessionState State
        {
            get
            {
                lock (_statesHistory)
                {
                    return _statesHistory
                        .OrderByDescending(x => x.Item2)
                        .First()
                        .Item1;
                }
            }
        }

        public List<(VaspSessionState, DateTime)> StatesHistory
        {
            get
            {
                lock (_statesHistory)
                {
                    return _statesHistory
                        .OrderBy(x => x.Item2)
                        .ToList();
                }
            }
        }

        private List<(VaspSessionState, DateTime)> _statesHistory;
        private OriginatorSession _originatorSession;
        private readonly VaspClient _originatorClient;
        private readonly VaspClient _beneficiaryClient;
        private readonly IVaspMessageHandler _vaspMessageHandler;

        private bool? _shouldAllowTransferRequest;

        private readonly SemaphoreSlim _transferRequestSemaphore = new SemaphoreSlim(0, 1);

        private TransferReply _transferRequestReply;
        public PendingTransferRequest PendingTransferRequest { get; private set; }

        public VaspSessionWrapper(
            string beneficiaryName,
            VaspClient originatorClient,
            VaspClient beneficiaryClient)
        {
            Id = Guid.NewGuid().ToString();
            _statesHistory = new List<(VaspSessionState, DateTime)>();

            SetState(VaspSessionState.Created);

            _originatorClient = originatorClient;
            _beneficiaryClient = beneficiaryClient;

            _vaspMessageHandler = new VaspMessageHandlerCallbacks(
                (vaspInfo) =>
                {
                    if (State == VaspSessionState.SessionRequested)
                    {
                        SetState(VaspSessionState.SessionConfirmed);

                        return Task.FromResult(true);
                    }
                    else
                    {
                        return Task.FromResult(false);
                    }
                },
                async (request, currentSession) =>
                {
                    SetState(VaspSessionState.WaitingForTransferRequestApproval);

                    PendingTransferRequest = new PendingTransferRequest
                    {
                        BeneficiaryName = beneficiaryName,
                        OriginatorName = request.Originator.Name,
                        OriginatorDateOfBirth = request.Originator.PlaceOfBirth.DateOfBirth,
                        Transfer = new VirtualAssetTransfer
                        {
                            TransferType = request.Transfer.TransferType,
                            TransferAmount = request.Transfer.Amount,
                            VirtualAssetType = request.Transfer.VirtualAssetType
                        },
                        PostalAddress = request.Originator.PostalAddress
                    };

                    await _transferRequestSemaphore.WaitAsync();

                    if (_shouldAllowTransferRequest.Value)
                    {
                        var message = new TransferReplyMessage(currentSession.SessionId,
                            TransferReplyMessage.TransferReplyMessageCode.TransferAccepted,
                            request.Originator,
                            new Beneficiary(beneficiaryName, request.Beneficiary.VAAN),
                            new TransferReply(
                                request.Transfer.VirtualAssetType,
                                request.Transfer.TransferType,
                                request.Transfer.Amount,
                                "0x0"),
                            request.VASP);

                        SetState(VaspSessionState.TransferRequestConfirmed);

                        return message;
                    }
                    else
                    {
                        var message = new TransferReplyMessage(currentSession.SessionId,
                            TransferReplyMessage.TransferReplyMessageCode.TransferDeclinedRequestNotValid,
                            request.Originator,
                            new Beneficiary(beneficiaryName, request.Beneficiary.VAAN),
                            new TransferReply(
                                request.Transfer.VirtualAssetType,
                                request.Transfer.TransferType,
                                request.Transfer.Amount,
                                "ignore"),
                            request.VASP);

                        SetState(VaspSessionState.TransferRequestDeclined);

                        return message;
                    }
                },
                (dispatch, currentSession) =>
                {
                    var message = new TransferConfirmationMessage(currentSession.SessionId,
                        TransferConfirmationMessage.TransferConfirmationMessageCode.TransferConfirmed,
                        dispatch.Originator,
                        dispatch.Beneficiary,
                        dispatch.Transfer,
                        dispatch.Transaction,
                        dispatch.VASP);

                    SetState(VaspSessionState.TransferConfirmed);

                    return Task.FromResult(message);
                });

            _beneficiaryClient.RunListener(_vaspMessageHandler);
        }

        public void StartSession(Originator originator, VirtualAssetssAccountNumber beneficiaryVaan,
            VirtualAssetType assetType, decimal amount)
        {
            if (State != VaspSessionState.Created)
            {
                throw new InvalidOperationException($"The session {Id} has already started.");
            }

            _originatorClient.CreateSessionAsync(originator, beneficiaryVaan)
                .ContinueWith(x =>
                {
                    _originatorSession = x.Result;

                    Transfer(assetType, amount);
                });

            SetState(VaspSessionState.SessionRequested);
        }

        private void Transfer(VirtualAssetType assetType, decimal amount)
        {
            if (State != VaspSessionState.SessionConfirmed)
            {
                throw new InvalidOperationException($"The session {Id} is not in a confirmed state.");
            }

            _originatorSession.TransferRequestAsync(new TransferInstruction()
                {
                    VirtualAssetTransfer = new VirtualAssetTransfer()
                    {
                        TransferType = TransferType.BlockchainTransfer,
                        VirtualAssetType = assetType,
                        TransferAmount = amount.ToString(CultureInfo.InvariantCulture)
                    }
                })
                .ContinueWith(x =>
                {
                    _transferRequestReply = x.Result.Transfer;

                    DispatchTransfer(new Transaction($"0x{Guid.NewGuid().ToString("N")}", DateTime.UtcNow, "0x0...a"));
                });

            SetState(VaspSessionState.TransferRequestSent);
        }

        public void ReplyToTransferRequest(bool shouldAllowTransferRequest)
        {
            if (State != VaspSessionState.WaitingForTransferRequestApproval)
            {
                throw new InvalidOperationException($"The session {Id} is not in a transfer waiting state.");
            }

            _shouldAllowTransferRequest = shouldAllowTransferRequest;

            _transferRequestSemaphore.Release();
        }

        private void DispatchTransfer(Transaction transaction)
        {
            if (State != VaspSessionState.TransferRequestConfirmed)
            {
                throw new InvalidOperationException($"The session {Id} didn't get transfer request confirmation.");
            }

            _originatorSession.TransferDispatchAsync(_transferRequestReply, transaction)
                .ContinueWith(async (x) =>
                {
                    await _originatorSession.TerminateAsync(
                        TerminationMessage.TerminationMessageCode.SessionClosedTransferOccured);

                    _originatorSession.Wait();
                    _originatorClient.Dispose();
                    _beneficiaryClient.Dispose();

                    SetState(VaspSessionState.Terminated);
                });

            SetState(VaspSessionState.TransferDispatched);
        }

        private void SetState(VaspSessionState state)
        {
            lock (_statesHistory)
            {
                _statesHistory.Add((state, DateTime.UtcNow));
            }
        }
    }

    public enum VaspSessionState
    {
        Created,
        SessionRequested,
        SessionConfirmed,
        TransferRequestSent,
        WaitingForTransferRequestApproval,
        TransferRequestConfirmed,
        TransferRequestDeclined,
        TransferDispatched,
        TransferConfirmed,
        Terminated
    }
}
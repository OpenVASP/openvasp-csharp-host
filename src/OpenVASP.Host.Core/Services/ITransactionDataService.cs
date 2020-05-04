using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using PlaceOfBirth = OpenVASP.Host.Core.Models.PlaceOfBirth;
using PostalAddress = OpenVASP.Host.Core.Models.PostalAddress;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Core.Services
{
    public interface ITransactionDataService
    {
        (Transaction, Originator) GenerateTransactionData(
            string originatorFullName,
            string originatorVaan,
            PlaceOfBirth originatorPlaceOfBirth,
            PostalAddress originatorPostalAddress,
            string beneficiaryFullName,
            string beneficiaryVaan,
            VirtualAssetType asset,
            decimal amount,
            NaturalPersonId[] naturalPersonIds,
            JuridicalPersonId[] juridicalPersonIds,
            string bic);

        VirtualAssetsAccountNumber CreateVirtualAssetsAccountNumber(string beneficiaryVaan);

        void FillTransactionData(Transaction transaction, TransferRequestMessage message);

        Originator GetOriginatorFromTx(Transaction transaction);

        Beneficiary GetBeneficiaryFromTx(Transaction transaction);
    }
}

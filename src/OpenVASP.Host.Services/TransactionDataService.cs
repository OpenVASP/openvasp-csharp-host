using System;
using System.Linq;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Core.Services;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using PlaceOfBirth = OpenVASP.Host.Core.Models.PlaceOfBirth;
using PostalAddress = OpenVASP.Host.Core.Models.PostalAddress;
using Transaction = OpenVASP.Host.Core.Models.Transaction;

namespace OpenVASP.Host.Services
{
    public class TransactionDataService : ITransactionDataService
    {
        public Transaction GenerateTransactionData(
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
            string bic,
            TransactionType type)
        {
            var sanitizedBeneficiaryVaan = beneficiaryVaan.Replace(" ", "");
            var sanitizedOriginatorVaan = originatorVaan.Replace(" ", "");

            var transaction = new Transaction
            {
                Status = TransactionStatus.Created,
                OriginatorPostalAddress = originatorPostalAddress,
                OriginatorPlaceOfBirth = originatorPlaceOfBirth,
                Amount = amount,
                Asset = asset,
                Id = Guid.NewGuid().ToString(),
                CreationDateTime = DateTime.UtcNow,
                BeneficiaryVaan = sanitizedBeneficiaryVaan,
                OriginatorVaan = sanitizedOriginatorVaan,
                OriginatorFullName = originatorFullName,
                BeneficiaryFullName = beneficiaryFullName,
                OriginatorJuridicalPersonIds = juridicalPersonIds,
                OriginatorBic = bic,
                OriginatorNaturalPersonIds = naturalPersonIds,
                Type = type
            };

            return transaction;
        }

        public VirtualAssetsAccountNumber CreateVirtualAssetsAccountNumber(string beneficiaryVaan)
        {
            var sanitizedBeneficiaryVaan = beneficiaryVaan.Replace(" ", "");
            var beneficiaryVaspCode = sanitizedBeneficiaryVaan.Substring(0, 8);
            var beneficiaryCustomerSpecificNumber = sanitizedBeneficiaryVaan.Substring(8, 14);

            return VirtualAssetsAccountNumber.Create(beneficiaryVaspCode, beneficiaryCustomerSpecificNumber);
        }

        public void FillTransactionData(Transaction transaction, TransferRequestMessage message)
        {
            if (message.Originator.PostalAddress != null)
            {
                transaction.OriginatorPostalAddress = new PostalAddress
                {
                    Street = message.Originator.PostalAddress.StreetName,
                    AddressLine = message.Originator.PostalAddress.AddressLine,
                    Building = message.Originator.PostalAddress.BuildingNumber,
                    Country = message.Originator.PostalAddress.Country,
                    PostCode = message.Originator.PostalAddress.PostCode,
                    Town = message.Originator.PostalAddress.TownName
                };
            }

            if (message.Originator.PlaceOfBirth != null)
            {
                transaction.OriginatorPlaceOfBirth = new PlaceOfBirth
                {
                    Country = message.Originator.PlaceOfBirth.CountryOfBirth,
                    Date = message.Originator.PlaceOfBirth.DateOfBirth,
                    Town = message.Originator.PlaceOfBirth.CityOfBirth
                };
            }

            transaction.OriginatorJuridicalPersonIds = message.Originator.JuridicalPersonId?.Select(
                    x => new JuridicalPersonId(x.Identifier, x.IdentificationType, x.IssuingCountry,
                        x.NonStateIssuer))
                .ToArray();
            transaction.OriginatorNaturalPersonIds = message.Originator.NaturalPersonId?.Select(
                    x => new NaturalPersonId(x.Identifier, x.IdentificationType, x.IssuingCountry,
                        x.NonStateIssuer))
                .ToArray();
            transaction.OriginatorBic = message.Originator.BIC;
            transaction.Amount = message.Transfer.Amount;
            transaction.Asset = message.Transfer.VirtualAssetType;
            transaction.BeneficiaryVaan = message.Beneficiary.VAAN.Replace(" ", "");
            transaction.OriginatorVaan = message.Originator.VAAN.Replace(" ", "");
            transaction.OriginatorFullName = message.Originator.Name;
            transaction.BeneficiaryFullName = message.Beneficiary.Name;
        }

        public Originator GetOriginatorFromTx(Transaction transaction)
        {
            return new Originator(
                transaction.OriginatorFullName,
                transaction.OriginatorVaan,
                new Messaging.Messages.Entities.PostalAddress(
                    transaction.OriginatorPostalAddress.Street,
                    transaction.OriginatorPostalAddress.Building,
                    transaction.OriginatorPostalAddress.AddressLine,
                    transaction.OriginatorPostalAddress.PostCode,
                    transaction.OriginatorPostalAddress.Town,
                    transaction.OriginatorPostalAddress.Country),
                transaction.OriginatorPlaceOfBirth != null
                    ? new Messaging.Messages.Entities.PlaceOfBirth(
                        transaction.OriginatorPlaceOfBirth.Date,
                        transaction.OriginatorPlaceOfBirth.Town,
                        transaction.OriginatorPlaceOfBirth.Country)
                    : null,
                transaction.OriginatorNaturalPersonIds,
                transaction.OriginatorJuridicalPersonIds,
                transaction.OriginatorBic);
        }

        public Beneficiary GetBeneficiaryFromTx(Transaction transaction)
        {
            return new Beneficiary(transaction.BeneficiaryFullName, transaction.BeneficiaryVaan);
        }
    }
}

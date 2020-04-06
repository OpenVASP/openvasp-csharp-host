using System;
using System.Globalization;
using System.Linq;
using OpenVASP.Messaging.Messages;
using OpenVASP.Messaging.Messages.Entities;
using OpenVASP.ProtocolMessages.Messages;

namespace OpenVASP.ProtoMappers.Mappers
{
    public static class Mapper
    {
        #region TO_PROTO

        public static ProtoMessage MapMessageToProto(MessageType messageType, Message message)
        {
            var proto = new ProtoMessage()
            {
                MessageCode = message.MessageCode,
                MessageId = message.MessageId,
                MessageType = (int)messageType,
                SessionId = message.SessionId
            };

            return proto;
        }

        public static ProtoVaspInfo MapVaspInformationToProto(VaspInformation vaspInfo)
        {
            var proto = new ProtoVaspInfo()
            {
                Name = vaspInfo.Name,
                PlaceOfBirth = MapPlaceOfBirthToProto(vaspInfo.PlaceOfBirth),
                PostalAddress = MapPostalAddressToProto(vaspInfo.PostalAddress),
                VaspIdentity = vaspInfo.VaspIdentity,
                VaspPubkey = vaspInfo.VaspPublickKey,
                Bic = vaspInfo.BIC ?? ""
            };

            var juridicalPeronsIds = vaspInfo
                .JuridicalPersonIds?.Select<JuridicalPersonId, ProtoJuridicalPersonId>(x =>
                MapJuridicalPersonIdToProto(x));

            var naturalPersonsIds =
                vaspInfo.NaturalPersonIds?.Select<NaturalPersonId, ProtoNaturalPersonId>(x =>
                    MapNaturalPersonIdToProto(x));

            if (juridicalPeronsIds != null)
            {
                proto.JuridicalPersonId.Add(juridicalPeronsIds);
            }

            if (naturalPersonsIds != null)
            {
                proto.NaturalPersonId.Add(naturalPersonsIds);
            }

            return proto;
        }

        public static ProtoNaturalPersonId MapNaturalPersonIdToProto(NaturalPersonId naturalPersonId)
        {
            if (naturalPersonId == null)
            {
                return null;
            }

            var proto = new ProtoNaturalPersonId()
            {
                IdentificationType = (int)naturalPersonId.IdentificationType,
                Identifier = naturalPersonId.Identifier,
                IssuingCountry = naturalPersonId.IssuingCountry.TwoLetterCode,
                NonstateIssuer = naturalPersonId.NonStateIssuer ?? string.Empty
            };

            return proto;
        }

        public static ProtoJuridicalPersonId MapJuridicalPersonIdToProto(JuridicalPersonId juridicalPersonId)
        {
            if (juridicalPersonId == null)
            {
                return null;
            }

            var proto = new ProtoJuridicalPersonId()
            {
                IdentificationType = (int)juridicalPersonId.IdentificationType,
                IssuingCountry = juridicalPersonId.IssuingCountry.TwoLetterCode,
                Identifier = juridicalPersonId.Identifier,
                NonstateIssuer = juridicalPersonId.NonStateIssuer
            };

            return proto;
        }

        public static ProtoPlaceOfBirth MapPlaceOfBirthToProto(PlaceOfBirth placeOfBirth)
        {
            if (placeOfBirth == null)
            {
                return null;
            }

            var proto = new ProtoPlaceOfBirth()
            {
                CityOfBirth = placeOfBirth.CityOfBirth,
                CountryOfBirth = placeOfBirth.CountryOfBirth.TwoLetterCode,
                Date = placeOfBirth.DateOfBirth.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
            };

            return proto;
        }

        public static ProtoPostalAddress MapPostalAddressToProto(PostalAddress postalAddress)
        {
            if (postalAddress == null)
            {
                return null;
            }

            var proto = new ProtoPostalAddress()
            {
                AddressLine = postalAddress.AddressLine,
                BuildingNumber = postalAddress.BuildingNumber,
                Country = postalAddress.Country.TwoLetterCode,
                PostCode = postalAddress.PostCode,
                StreetName = postalAddress.StreetName,
                TownName = postalAddress.TownName
            };

            return proto;
        }

        public static ProtoTransferRequest MapTransferToProto(TransferRequest messageTransfer)
        {
            if (messageTransfer == null)
                return null;

            var proto = new ProtoTransferRequest()
            {
                Amount = messageTransfer.Amount,
                TransferType = (int)messageTransfer.TransferType,
                VirtualAssetName = messageTransfer.VirtualAssetType.ToString()
            };

            return proto;
        }

        public static ProtoTransferReply MapTransferToProto(TransferReply messageTransfer)
        {
            if (messageTransfer == null)
                return null;

            var proto = new ProtoTransferReply()
            {
                Amount = messageTransfer.Amount,
                TransferType = (int)messageTransfer.TransferType,
                VirtualAssetName = messageTransfer.VirtualAssetType.ToString(),
                DestinationAddress = messageTransfer.DestinationAddress
            };

            return proto;
        }

        public static ProtoBeneficiary MapBeneficiaryToProto(Beneficiary messageBeneficiary)
        {
            if (messageBeneficiary == null)
                return null;

            var proto = new ProtoBeneficiary()
            {
                Name = messageBeneficiary.Name,
                Vaan = messageBeneficiary.VAAN,
            };

            return proto;
        }

        public static ProtoOriginator MapOriginatorToProto(Originator messageOriginator)
        {
            if (messageOriginator == null)
                return null;

            var proto = new ProtoOriginator()
            {
                Name = messageOriginator.Name,
                Vaan = messageOriginator.VAAN,
                Bic = messageOriginator.BIC,
                PlaceOfBirth = MapPlaceOfBirthToProto(messageOriginator.PlaceOfBirth),
                PostalAddress = MapPostalAddressToProto(messageOriginator.PostalAddress)
            };

            if (messageOriginator.JuridicalPersonId != null &&
                messageOriginator.JuridicalPersonId.Any())
            {
                proto.JuridicalPersonId.AddRange(messageOriginator.JuridicalPersonId.Select(x => MapJuridicalPersonIdToProto(x)));
            }

            if (messageOriginator.NaturalPersonId != null &&
                messageOriginator.NaturalPersonId.Any())
            {
                proto.NaturalPersonId.AddRange(messageOriginator.NaturalPersonId.Select(x => MapNaturalPersonIdToProto(x)));
            }

            return proto;
        }

        public static ProtoTransaction MapTranactionToProto(Transaction messageTransaction)
        {
            if (messageTransaction == null)
                return null;

            var proto = new ProtoTransaction()
            {
                SendingAddress = messageTransaction.SendingAddress,
                TransactionDatetime = messageTransaction.DateTime.ToUniversalTime()
                    .ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture),
                TransactionId = messageTransaction.TransactionId
            };

            return proto;
        }

        #endregion

        #region FROM_PROTO

        public static VaspInformation MapVaspInformationFromProto(ProtoVaspInfo vaspInfo)
        {
            var proto = new VaspInformation(
                vaspInfo.Name,
                vaspInfo.VaspIdentity,
                vaspInfo.VaspPubkey,
                MapPostalAddressFromProto(vaspInfo.PostalAddress),
                MapPlaceOfBirthFromProto(vaspInfo.PlaceOfBirth),
                vaspInfo.NaturalPersonId?.Select<ProtoNaturalPersonId, NaturalPersonId>(x => MapNaturalPersonIdFromProto(x)).ToArray(),
                vaspInfo.JuridicalPersonId?.Select<ProtoJuridicalPersonId, JuridicalPersonId>(x => MapJuridicalPersonIdFromProto(x)).ToArray(),
                string.IsNullOrEmpty(vaspInfo.Bic) ? null : vaspInfo.Bic);

            return proto;
        }

        public static NaturalPersonId MapNaturalPersonIdFromProto(ProtoNaturalPersonId naturalPersonId)
        {
            if (naturalPersonId == null)
            {
                return null;
            }

            Country.List.TryGetValue(naturalPersonId.IssuingCountry, out var country);
            var proto = new NaturalPersonId(
                naturalPersonId.Identifier,
                (NaturalIdentificationType)naturalPersonId.IdentificationType,
                country,
                naturalPersonId.NonstateIssuer);

            return proto;
        }

        public static JuridicalPersonId MapJuridicalPersonIdFromProto(ProtoJuridicalPersonId juridicalPersonId)
        {
            if (juridicalPersonId == null)
            {
                return null;
            }

            Country.List.TryGetValue(juridicalPersonId.IssuingCountry, out var country);
            var proto = new JuridicalPersonId(
                juridicalPersonId.Identifier,
                (JuridicalIdentificationType)juridicalPersonId.IdentificationType,
                country,
                juridicalPersonId.NonstateIssuer);

            return proto;
        }

        public static PlaceOfBirth MapPlaceOfBirthFromProto(ProtoPlaceOfBirth placeOfBirth)
        {
            if (placeOfBirth == null)
            {
                return null;
            }

            Country.List.TryGetValue(placeOfBirth.CountryOfBirth, out var country);
            DateTime.TryParseExact(placeOfBirth.Date, "yyyyMMdd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth);
            var proto = new PlaceOfBirth(dateOfBirth, placeOfBirth.CityOfBirth, country);

            return proto;
        }

        public static PostalAddress MapPostalAddressFromProto(ProtoPostalAddress postalAddress)
        {
            if (postalAddress == null)
            {
                return null;
            }

            Country.List.TryGetValue(postalAddress.Country, out var country);
            var proto = new PostalAddress(
                postalAddress.StreetName,
                postalAddress.BuildingNumber,
                postalAddress.AddressLine,
                postalAddress.PostCode,
                postalAddress.TownName,
                country);

            return proto;
        }

        public static Originator MapOriginatorFromProto(ProtoOriginator messageOriginator)
        {
            if (messageOriginator == null)
                return null;

            var obj = new Originator(
                messageOriginator.Name,
                messageOriginator.Vaan,
                MapPostalAddressFromProto(messageOriginator.PostalAddress),
                MapPlaceOfBirthFromProto(messageOriginator.PlaceOfBirth),
                messageOriginator.NaturalPersonId?.Select(x => MapNaturalPersonIdFromProto(x))?.ToArray(),
                messageOriginator.JuridicalPersonId?.Select(x => MapJuridicalPersonIdFromProto(x))?.ToArray(),
                messageOriginator.Bic
            );

            return obj;
        }

        public static Beneficiary MapBeneficiaryFromProto(ProtoBeneficiary messageBeneficiary)
        {
            if (messageBeneficiary == null)
                return null;

            var obj = new Beneficiary(
                messageBeneficiary.Name,
                messageBeneficiary.Vaan);

            return obj;
        }

        public static TransferRequest MapTransferFromProto(ProtoTransferRequest messageTransfer)
        {
            if (messageTransfer == null)
                return null;

            Enum.TryParse(messageTransfer.VirtualAssetName, out VirtualAssetType assetType);

            var obj = new TransferRequest(assetType, (TransferType)messageTransfer.TransferType, messageTransfer.Amount);

            return obj;
        }

        public static TransferReply MapTransferFromProto(ProtoTransferReply messageTransfer)
        {
            if (messageTransfer == null)
                return null;

            Enum.TryParse(messageTransfer.VirtualAssetName, out VirtualAssetType assetType);

            var obj = new TransferReply(
                assetType, 
                (TransferType)messageTransfer.TransferType, 
                messageTransfer.Amount,
                messageTransfer.DestinationAddress);

            return obj;
        }

        public static Transaction MapTranactionFromProto(ProtoTransaction messageTransaction)
        {
            if (messageTransaction == null)
                return null;

            DateTime.TryParseExact(messageTransaction.TransactionDatetime, "yyyy-MM-ddTHH:mm:ssZ",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime);

            dateTime = dateTime.ToUniversalTime();

            var obj = new Transaction(messageTransaction.TransactionId, dateTime, messageTransaction.SendingAddress);

            return obj;
        }

        #endregion

    }
}
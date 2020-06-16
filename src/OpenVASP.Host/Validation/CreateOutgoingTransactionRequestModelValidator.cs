using System.Linq;
using FluentValidation;
using OpenVASP.Host.Models.Request;

namespace OpenVASP.Host.Validation
{
    public class CreateOutgoingTransactionRequestModelValidator
        : AbstractValidator<CreateOutgoingTransactionRequestModel>
    {
        public CreateOutgoingTransactionRequestModelValidator()
        {
            RuleFor(x => x.Asset)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.Asset)} required");

            RuleFor(x => x.Amount)
                .Must(x => x > 0)
                .WithMessage(x => $"{nameof(x.Asset)} must be a positive number");

            RuleFor(x => x)
                .Must(x =>
                    x.OriginatorPlaceOfBirth != null
                    && x.OriginatorNaturalPersonIds != null && x.OriginatorNaturalPersonIds.Length > 0
                    && (x.OriginatorJuridicalPersonIds == null || x.OriginatorJuridicalPersonIds.Length == 0)
                    && string.IsNullOrWhiteSpace(x.OriginatorBic)
                    || x.OriginatorJuridicalPersonIds != null && x.OriginatorJuridicalPersonIds.Length > 0
                    && x.OriginatorPlaceOfBirth == null
                    && (x.OriginatorNaturalPersonIds == null || x.OriginatorNaturalPersonIds.Length == 0)
                    && string.IsNullOrWhiteSpace(x.OriginatorBic)
                    || !string.IsNullOrWhiteSpace(x.OriginatorBic)
                    && x.OriginatorPlaceOfBirth == null
                    && (x.OriginatorNaturalPersonIds == null || x.OriginatorNaturalPersonIds.Length == 0)
                    && (x.OriginatorJuridicalPersonIds == null || x.OriginatorJuridicalPersonIds.Length == 0))
                .WithMessage(x => "Originator needs to be either a bank or a natural person or a juridical person.");

            When(x => x.OriginatorPlaceOfBirth != null, () =>
            {
                RuleFor(x => x.OriginatorPlaceOfBirth.CountryIso2Code)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPlaceOfBirth.CountryIso2Code)} required")
                .Must(x => x.Length == 2)
                .WithMessage(x => $"{nameof(x.OriginatorPlaceOfBirth.CountryIso2Code)} must be ISO2 code");
            });

            When(x => x.OriginatorNaturalPersonIds != null && x.OriginatorNaturalPersonIds.Length > 0, () =>
            {
                RuleFor(x => x.OriginatorNaturalPersonIds)
                .Must(x => x.All(i => !string.IsNullOrWhiteSpace(i.CountryIso2Code) && i.CountryIso2Code.Length == 2))
                .WithMessage(x => "All natural persons must have ISO2 code for a country code");
            });

            When(x => x.OriginatorJuridicalPersonIds != null && x.OriginatorJuridicalPersonIds.Length > 0, () =>
            {
                RuleFor(x => x.OriginatorJuridicalPersonIds)
                .Must(x => x.All(i => !string.IsNullOrWhiteSpace(i.CountryIso2Code) && i.CountryIso2Code.Length == 2))
                .WithMessage(x => "All juridical persons must have ISO2 code for a country code");
            });

            RuleFor(x => x.OriginatorPostalAddress)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress)} required");

            When(x => x.OriginatorPostalAddress != null, () =>
            {
                RuleFor(x => x.OriginatorPostalAddress.CountryIso2Code)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress.CountryIso2Code)} required")
                .Must(x => x.Length == 2)
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress.CountryIso2Code)} must be ISO2 code");

                RuleFor(x => x.OriginatorPostalAddress.Building)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress.Building)} required");

                RuleFor(x => x.OriginatorPostalAddress.Street)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress.Street)} required");

                RuleFor(x => x.OriginatorPostalAddress.Town)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorPostalAddress.Town)} required");
            });

            RuleFor(x => x.OriginatorFullName)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorFullName)} required");

            RuleFor(x => x.OriginatorVaan)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.OriginatorVaan)} required");

            RuleFor(x => x.BeneficiaryFullName)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.BeneficiaryFullName)} required");

            RuleFor(x => x.BeneficiaryVaan)
                .NotEmpty()
                .WithMessage(x => $"{nameof(x.BeneficiaryVaan)} required");
        }
    }
}

using System;
using AutoMapper;
using OpenVASP.Host.Core.Models;
using OpenVASP.Host.Models.Response;

namespace OpenVASP.Host
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Transaction, TransactionDetailsModel>(MemberList.Destination)
                .ForMember(e => e.TransactionType, opt => opt.MapFrom(c => c.Type.ToString()))
                .ForMember(e => e.Asset, opt => opt.MapFrom(c => c.Asset.ToString()))
                .ForMember(e => e.CounterPartyVaspName, opt => opt.MapFrom(c => c.CounterPartyVasp == null ? null : c.CounterPartyVasp.Name))
                .ForMember(
                    e => e.OriginatorPostalAddressStreet,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.Street))
                .ForMember(
                    e => e.OriginatorPostalAddressBuilding,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.Building))
                .ForMember(
                    e => e.OriginatorPostalAddressAddressLine,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.AddressLine))
                .ForMember(
                    e => e.OriginatorPostalAddressPostCode,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.PostCode))
                .ForMember(
                    e => e.OriginatorPostalAddressTown,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.Town))
                .ForMember(
                    e => e.OriginatorPostalAddressCountryIso2Code,
                    opt => opt.MapFrom(c => c.OriginatorPostalAddress == null ? null : c.OriginatorPostalAddress.Country))
                .ForMember(
                    e => e.OriginatorPlaceOfBirthTown,
                    opt => opt.MapFrom(c => c.OriginatorPlaceOfBirth == null ? null : c.OriginatorPlaceOfBirth.Town))
                .ForMember(
                    e => e.OriginatorPlaceOfBirthCountryIso2Code,
                    opt => opt.MapFrom(c => c.OriginatorPlaceOfBirth == null ? null : c.OriginatorPlaceOfBirth.Country))
                .ForMember(
                    e => e.OriginatorPlaceOfBirthDate,
                    opt => opt.MapFrom(c => c.OriginatorPlaceOfBirth == null ? default : c.OriginatorPlaceOfBirth.Date))
                .ForMember(e => e.Status, opt => opt.MapFrom(c => c.Status.ToString()));
        }
    }
}

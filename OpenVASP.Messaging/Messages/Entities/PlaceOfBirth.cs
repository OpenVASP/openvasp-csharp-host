using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenVASP.Messaging.Messages.Entities
{
    public class PlaceOfBirth
    {
        public PlaceOfBirth(DateTime dateOfBirth, string cityOfBirth, Country countryOfBirth)
        {
            DateOfBirth = dateOfBirth;
            CityOfBirth = cityOfBirth;
            CountryOfBirth = countryOfBirth;
        }

        [JsonProperty("birthdate")]
        [JsonConverter(typeof(DateFormatConverter), "yyyyMMdd")]
        public DateTime DateOfBirth { get; private set; }

        [JsonProperty("birthcity")]
        public string CityOfBirth { get; private set; }

        [JsonProperty("birthcountry")]
        [JsonConverter(typeof(CountryConverter))]
        public Country CountryOfBirth { get; private set; }
    }
    
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
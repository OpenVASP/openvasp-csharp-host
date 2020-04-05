using System;

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

        public DateTime DateOfBirth { get; private set; }

        public string CityOfBirth { get; private set; }

        public Country CountryOfBirth { get; private set; }
    }
}
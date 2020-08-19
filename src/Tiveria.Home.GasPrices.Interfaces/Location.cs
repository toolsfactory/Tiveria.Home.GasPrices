namespace Tiveria.Home.GasPrices.Interfaces
{
    public class Location
    {
        public double Latitude { get; private set; }
        public double Longtitude { get; private set; }
        public string City { get; private set; }
        public string Street { get; private set; }
        public string HouseNumber { get; private set; }
        public int PostCode { get; private set; }

        public Location(double lat, double lng, string city, string street, string houseNumber, int postCode)
        {
            Latitude = lat;
            Longtitude = lng;
            City = city;
            Street = street;
            HouseNumber = houseNumber;
            PostCode = postCode;
        }
        public override string ToString()
        {
            return $"{Street}, {HouseNumber}\n{PostCode} {City}";
        }
    }
 }

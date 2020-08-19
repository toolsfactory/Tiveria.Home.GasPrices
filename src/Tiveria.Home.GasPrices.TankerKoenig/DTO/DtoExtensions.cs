using System.Collections.Generic;
using Tiveria.Home.GasPrices.Interfaces;

namespace Tiveria.Home.GasPrices.TankerKoenig.DTO
{
    internal static class DtoExtensions
    {
        internal static GasStationDetails MapToBusiness(this StationDto source)
        {
            var gasprices = new Dictionary<GasType, float?>(3);
            gasprices.Add(GasType.Diesel, source.Diesel);
            gasprices.Add(GasType.E5, source.E5);
            gasprices.Add(GasType.E10, source.E10);
            return new GasStationDetails()
            {
                ID = source.Id,
                Brand = source.Brand,
                Name = source.Name,
                Open = source.IsOpen,
                Distance = source.Dist,
                Location = new Location(source.Lat, source.Lng, source.City, source.Street, source.HouseNumber, source.PostCode),
                GasPrices = gasprices
            };
        }

        internal static Dictionary<GasType, float?> MapToBusiness(this PriceDto source)
        {
            var gasprices = new Dictionary<GasType, float?>(3);
            gasprices.Add(GasType.Diesel, source.Diesel);
            gasprices.Add(GasType.E5, source.E5);
            gasprices.Add(GasType.E10, source.E10);
            return gasprices;
        }
    }

}

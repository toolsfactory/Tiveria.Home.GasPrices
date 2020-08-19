using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tiveria.Home.GasPrices.Interfaces
{
    public interface IGasStations : IEnumerable<GasStationDetails>
    {
        int Count { get; }

        GasStationDetails this[string id] { get; }

        Task<bool> InitializeAsync(double latitude, double longitude, double maxDistanceKM = 5, GasType gasTypes = GasType.All);
        Task<bool> InitializeAsync(string[] ids);
        Task<bool> UpdatePricesAsync();
    }
 }

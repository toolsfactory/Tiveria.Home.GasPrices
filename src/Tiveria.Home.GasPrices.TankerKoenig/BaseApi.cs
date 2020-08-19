using Tiveria.Home.GasPrices.Interfaces;
using System.Threading.Tasks;
using Tiveria.Home.GasPrices.TankerKoenig.DTO;
using System;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace Tiveria.Home.GasPrices.TankerKoenig
{
    public class BaseApi
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;

        public Guid ApiKey { get; }
        public Uri BaseUri { get; }

        public enum SortBy
        {
            Price,
            Distance
        }

        public BaseApi(Guid apikey, Uri baseUri, IHttpClientFactory httpClientFactory, ILogger logger)
        {
            ApiKey = apikey;
            BaseUri = baseUri;
            _logger = logger;
            _client = httpClientFactory.CreateClient("tanker");
        }

        public async Task<StationListDto> SearchGasStationsAsync(double latitude, double longitude, byte maxDistanceKM = 5, GasType gasTypes = GasType.All, SortBy sortby = SortBy.Distance)
        {
            VerifySearchGasStationsParameters(latitude, longitude, maxDistanceKM);
            return await SearchGasStationsInternalAsync(latitude, longitude, maxDistanceKM, gasTypes, sortby);
        }

        private void VerifySearchGasStationsParameters(double latitude, double longitude, byte maxDistanceKM)
        {
            if (Math.Abs(latitude) > 90)
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be in range of -90 to 90");
            if (Math.Abs(longitude) > 180)
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be in range of -180 to 180");
            if (maxDistanceKM > 25)
                throw new ArgumentOutOfRangeException(nameof(maxDistanceKM), "Distance must be 25 or below");
        }

        private async Task<StationListDto> SearchGasStationsInternalAsync(double latitude, double longitude, byte maxDistanceKM, GasType gasTypes, SortBy sortby)
        {
            var response = await _client.GetAsync(GenerateRequestUrl());
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StationListDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true });

            string TranslateGasType(GasType gt)
            => gt switch
            {
                GasType.Diesel => "diesel",
                GasType.E5 => "e5",
                GasType.E10 => "e10",
                _ => "all"
            };

            string GenerateRequestUrl()
            {
                var c = CultureInfo.InvariantCulture;
                var latInv = latitude.ToString(c);
                var lngInv = longitude.ToString(c);
                var distInv = maxDistanceKM.ToString(c);
                var uri = new Uri(BaseUri, $"/json/list.php?lat={latInv}&lng={lngInv}&sort=dist&rad={distInv}&type={TranslateGasType(gasTypes)}&apikey={ApiKey}");
                return uri.ToString();
            }
        }

        public async Task<StationDetailsDto> GetGasStationsDetailsAsync(Guid id)
        {
            return await GetGasStationsDetailsInternalAsync(id);
        }


        private async Task<StationDetailsDto> GetGasStationsDetailsInternalAsync(Guid id)
        {
            var response = await _client.GetAsync(GenerateRequestUrl());
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<StationDetailsDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true });

            string GenerateRequestUrl()
            {
                var uri = new Uri(BaseUri, $"/json/detail.php?lid={id}&apikey={ApiKey}");
                return uri.ToString();
            }
        }

        public async Task<PriceListDto> GetPriceListAsync(string[] ids)
        {
            VerifyIds();

            var response = await _client.GetAsync(GenerateRequestUrl());
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PriceListDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true });

            string GenerateRequestUrl()
            {
                var idsparam = String.Join(',', ids);
                var uri = new Uri(BaseUri, $"/json/prices.php?ids={idsparam}&apikey={ApiKey}");
                return uri.ToString();
            }

            void VerifyIds()
            {
                foreach (var id in ids)
                {
                    if (!Guid.TryParse(id, out var _))
                    { throw new ArgumentException($"'{id}' is not a valid Guid"); }
                }
            }
        }

        public async Task<PriceListDto> GetPriceListAsync(Guid[] ids)
        {
            var response = await _client.GetAsync(GenerateRequestUrl());
            var body = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PriceListDto>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true });

            string GenerateRequestUrl()
            {
                var idsparam = String.Join(',', ids);
                var uri = new Uri(BaseUri, $"/json/prices.php?ids={idsparam}&apikey={ApiKey}");
                return uri.ToString();
            }
        }

    }
}

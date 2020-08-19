using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tiveria.Home.GasPrices.TankerKoenig;
using Tiveria.Home.GasPrices.TankerKoenig.DTO;

namespace Tiveria.Home.GasPrices.Service
{
    class GasPricesService : BackgroundService
    {
        private readonly ILogger<GasPricesService> _logger;
        private readonly IOptions<ServiceOptions> _options;
        private readonly HttpClient _client;
        private readonly BaseApi _api;
        private readonly Uri _openHabUri;
        private readonly Random _rand;
        private readonly int _delayvar;

        public GasPricesService(ILogger<GasPricesService> logger, IHttpClientFactory httpClientFactory, IOptions<ServiceOptions> options, IOptions<TankerKoenigManagerOptions> tkoptions)
        {
            _logger = logger;
            _options = options;
            _client = httpClientFactory.CreateClient("GasPricesService");
            _api = new BaseApi(tkoptions.Value.ApiKey, new Uri(tkoptions.Value.BaseUrl), httpClientFactory, logger);
            _openHabUri = new Uri(_options.Value.Host);
            _rand = new Random(564);
            _delayvar = _options.Value.UpdateDelaySeconds / 10;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Service is starting.");

            stoppingToken.Register(() =>
                _logger.LogDebug("Service stop request received."));

            _logger.LogInformation($"Startup delay configured: {_options.Value.StartupDelaySeconds} sec");
            await Task.Delay(millisecondsDelay: _options.Value.StartupDelaySeconds * 1000, cancellationToken: stoppingToken).ConfigureAwait(false);

            var ids = _options.Value.GasStations.Select(x => x.StationId).ToArray();
            while (!stoppingToken.IsCancellationRequested)
            {
                var data = await _api.GetPriceListAsync(ids).ConfigureAwait(false);
                await PushInfoToOpenhabAsync(data, stoppingToken).ConfigureAwait(false);
                var nextdelay = GenerateNextDelay();
                _logger.LogInformation($"Next update in : {nextdelay/1000} sec");
                await Task.Delay(millisecondsDelay: nextdelay, cancellationToken: stoppingToken).ConfigureAwait(false);
            }

            await Task.Delay(1000).ConfigureAwait(false);
            _logger.LogDebug("Service is finished.");

            int GenerateNextDelay()
            {
                return (_options.Value.UpdateDelaySeconds + _rand.Next(-_delayvar, _delayvar)) * 1000;
            }
        }

        private async Task PushInfoToOpenhabAsync(PriceListDto data, CancellationToken token)
        {
            foreach(var item in data.Prices)
            {
                var itemdetails = _options.Value.GasStations.FirstOrDefault(x => x.StationId.ToUpperInvariant() == item.Key.ToUpperInvariant());
                if (itemdetails != null)
                {
                    await CheckAndHandleItemAsync(itemdetails.DieselItem, item.Value.Diesel, token).ConfigureAwait(false);
                    await CheckAndHandleItemAsync(itemdetails.E5Item, item.Value.E5, token).ConfigureAwait(false);
                    await CheckAndHandleItemAsync(itemdetails.E10Item, item.Value.E10, token).ConfigureAwait(false);
                }
            }
            await SendAsync(GenerateUriForItem(_options.Value.ItemLastUpdate), DateTime.Now.ToString("s", CultureInfo.InvariantCulture.DateTimeFormat), token).ConfigureAwait(false);
        }

        private async Task CheckAndHandleItemAsync(string item, float? price, CancellationToken token)
        {
            if (!String.IsNullOrEmpty(item) && price.HasValue)
            {
                await SendAsync(GenerateUriForItem(item), price.Value.ToString(CultureInfo.InvariantCulture), token).ConfigureAwait(false);
            }
        }

        private Uri GenerateUriForItem(string item)
        {
            var path = String.Format(CultureInfo.InvariantCulture, _options.Value.Basepath, item);
            return new Uri(_openHabUri, path);
        }


        private async Task SendAsync(Uri endpoint, string value, CancellationToken token)
        {
            try
            {
                _logger.LogInformation($"Sending '{value}' to Endpoint '{endpoint}'");
                using var content = new StringContent(value);
                await _client.PutAsync(endpoint, content, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed sending message to endpoint '{endpoint}'", e);
            }
        }

    }
}

using System.Collections.Generic;

namespace Tiveria.Home.GasPrices.TankerKoenig.DTO
{
    public class PriceListDto : RequestStatusDto
    {
        public IDictionary<string, PriceDto> Prices { get; set; }
    }
}

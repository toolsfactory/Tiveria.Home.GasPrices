using System;
using System.Collections.Generic;

namespace Tiveria.Home.GasPrices.Service
{
    public class ServiceOptions
    {
        public UInt16 StartupDelaySeconds { get; set; }
        public UInt16 UpdateDelaySeconds { get; set; }
        public GasStationSettings[] GasStations { get; set; }
        public string Host { get; set; }
        public string Basepath { get; set; }
        public string ItemLastUpdate { get; set; }
    }

    public class GasStationSettings
    {
        public string StationId { get; set; }
        public string DieselItem { get; set; }
        public string E5Item { get; set; }
        public string E10Item { get; set; }
    }

}
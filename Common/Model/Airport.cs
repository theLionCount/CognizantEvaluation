using Newtonsoft.Json;
using System;

namespace Common.Model
{
    public class Airport
    {
        public int Id { get; set; }
        public string iata { get; set; }
        public string name { get; set; }
        public string iso { get; set; }
        
        [JsonProperty("lon")]
        public double? longitude { get; set; }
        
        [JsonProperty("lat")]
        public double? lattitude { get; set; }
        public Size? size { get; set; }
        public AirportType type { get; set; }
        public int status { get; set; }
        public Continent continent { get; set; }
    }
}

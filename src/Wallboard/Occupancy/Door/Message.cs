using System.Text.Json.Serialization;

namespace Wallboard.Occupancy.Door
{
    public class Message
    {
        [JsonPropertyName("illuminance")]
        public int Illuminance { get; set; }

        [JsonPropertyName("linkquality")]
        public int LinkQuality { get; set; }
        
        [JsonPropertyName("occupancy")]
        public bool Occupancy { get; set; }
        
        [JsonPropertyName("battery")]
        public int Battery { get; set; }
        
        [JsonPropertyName("voltage")]
        public int Voltage { get; set; }
    }
}

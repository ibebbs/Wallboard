using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wallboard.Occupancy.Presence
{
    public class Message
    {
        public static Message Create(Wallboard.Message source)
        {
            return JsonSerializer.Deserialize<Message>(source.Payload);
        }

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

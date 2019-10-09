using System.ComponentModel.DataAnnotations;

namespace Wallboard.Mqtt
{
    public class Config
    {
        [Required(ErrorMessage = "Mqtt broker address must be supplied")]
        public string Broker { get; set; }

        public int Port { get; set; } = 1883;
    }
}

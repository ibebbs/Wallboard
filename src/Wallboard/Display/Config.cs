using System.ComponentModel.DataAnnotations;

namespace Wallboard.Display
{
    public class Config
    {
        [Required(ErrorMessage = "Display controller port must be supplied and be the name of a valid serial port (i.e. 'COM1' on Windows, '/dev/ttyAMA0' on linux)")]
        public string Port { get; set; }
    }
}

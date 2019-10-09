using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Mdc = Bebbs.Mdc;
using Command = Bebbs.Mdc.Command;

namespace Wallboard.Display
{
    public interface IController
    {
        ValueTask StartAsync();

        ValueTask StopAsync();

        ValueTask PowerOn();

        ValueTask PowerOff();
    }

    public class Controller : IController
    {
        private readonly IOptions<Config> _configuration;
        private readonly ILogger<Controller> _logger;

        private Mdc.IConnection _mdc;

        public Controller(IOptions<Config> configuration, ILogger<Controller> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async ValueTask StartAsync()
        {
            if (_mdc == null)
            {
                _logger.LogInformation($"Connecting to display on port '{_configuration.Value.Port}'");

                _mdc = Mdc.Connection.Factory.ForSerialPort(_configuration.Value.Port);
                await _mdc.ConnectAsync();
            }
            else
            {
                throw new InvalidOperationException("Controller has already been started");
            }
        }

        public async ValueTask StopAsync()
        {
            if (_mdc != null)
            {
                _logger.LogInformation($"Disconnecting from display on port '{_configuration.Value.Port}'");

                await _mdc.DisconnectAsync();
                await _mdc.DisposeAsync();

                _mdc = null;
            }
            else
            {
                throw new InvalidOperationException("Controller has not been started");
            }
        }

        public async ValueTask PowerOn()
        {
            _logger.LogInformation($"Powering on display on port '{_configuration.Value.Port}'");

            await _mdc.IssueAsync(Command.Power.On(0));

            _logger.LogInformation($"Successfully powered on display on port '{_configuration.Value.Port}'");
        }

        public async ValueTask PowerOff()
        {
            _logger.LogInformation($"Powering off display on port '{_configuration.Value.Port}'");

            await _mdc.IssueAsync(Command.Power.Off(0));

            _logger.LogInformation($"Successfully powered off display on port '{_configuration.Value.Port}'");
        }
    }
}

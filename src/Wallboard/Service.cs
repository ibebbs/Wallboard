using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wallboard
{
    public class Service : IHostedService
    {
        private readonly Display.IController _controller;
        private readonly Mqtt.IConnection _connection;
        private readonly ILogger<Service> _logger;

        private IDisposable _subscription;

        public Service(Display.IController controller, Mqtt.IConnection connection, ILogger<Service> logger)
        {
            _controller = controller;
            _connection = connection;
            _logger = logger;
        }

        private async Task<Unit> PowerOn()
        {
            await _controller.PowerOn();

            return Unit.Default;
        }

        private async Task<Unit> PowerOff()
        {
            await _controller.PowerOff();

            return Unit.Default;
        }

        private void Log(Occupancy.State occupancy)
        {
            _logger.LogInformation($"Occupancy changed to '{occupancy}'");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _subscription = Occupancy.Logic
                .WhenOccupancyChanges(_connection.Messages)
                .Do(Log)
                .Select(occupancy => occupancy == Occupancy.State.Abscent ? new Func<Task<Unit>>(PowerOff) : new Func<Task<Unit>>(PowerOn))
                .SelectMany(async action => await action())
                .Subscribe(_ => { }, exception => _logger.LogCritical(exception, $"Critical error while processing message: '{exception.Message}'"));

            await _controller.StartAsync();
            await _connection.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
            }

            await _connection.StopAsync();
            await _controller.StopAsync();
        }
    }
}

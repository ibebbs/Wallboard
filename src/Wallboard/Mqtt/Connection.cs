using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mqtt;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Wallboard.Mqtt
{
    public interface IConnection
    {
        ValueTask StartAsync();

        ValueTask StopAsync();

        IObservable<Message> Messages { get; }
    }

    public class Connection : IConnection
    {
        private readonly IOptions<Config> _configuration;
        private readonly ILogger<Connection> _logger;
        private readonly Subject<Message> _messages;

        private IMqttClient _client;
        private IDisposable _subscription;

        public Connection(IOptions<Config> configuration, ILogger<Connection> logger)
        {
            _configuration = configuration;
            _logger = logger;

            _messages = new Subject<Message>();
        }

        private static IEnumerable<Message> Parse(MqttApplicationMessage message)
        {
            return message.Topic
                .Split('/')
                .Skip(1)
                .Select(device => new Message { Device = device, Payload = Encoding.UTF8.GetString(message.Payload) })
                .Take(1);
        }

        private void LogRaw(MqttApplicationMessage message)
        {
            _logger.LogDebug($"Received Mqtt message on topic '{message.Topic}'");
        }

        private void LogMessage(Message message)
        {
            _logger.LogInformation($"Parsed Mqtt message from device '{message.Device}' with payload '{message.Payload}'");
        }

        public async ValueTask StartAsync()
        {
            _logger.LogInformation($"Connecting to Mqtt broker at '{_configuration.Value.Broker}' on port '{_configuration.Value.Port}'");

            _client = await MqttClient.CreateAsync(_configuration.Value.Broker, _configuration.Value.Port);
            await _client.ConnectAsync();

            // Lets get the payload of messages as a string
            _subscription = _client.MessageStream
                .Do(LogRaw)
                .SelectMany(Parse)
                .Do(LogMessage)
                .Subscribe(_messages);

            _logger.LogInformation($"Subscribing to topic 'zigbee2mqtt/+' from broker at '{_configuration.Value.Broker}' on port '{_configuration.Value.Port}'");

            // Message topics will be in the form of "zigbee2mqtt/RTCGQ11M_office_shelves" so let's subscribe to
            // anything at the "zigbee2mqtt" root
            await _client.SubscribeAsync(@"zigbee2mqtt/+", MqttQualityOfService.AtMostOnce);
        }

        public async ValueTask StopAsync()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (_client != null)
            {
                await _client.UnsubscribeAsync("zigbee2mqtt/+");
                await _client.DisconnectAsync();

                _client.Dispose();
                _client = null;
            }
        }

        public IObservable<Message> Messages => _messages;
    }
}

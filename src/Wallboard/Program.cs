using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Wallboard
{
    class Program
    {

        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                    {
                        config.AddEnvironmentVariables("Wallboard:");
                        if (args != null)
                        {
                            config.AddCommandLine(args);
                        }
                    })
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddOptions<Display.Config>().ValidateDataAnnotations().Bind(hostContext.Configuration.GetSection("Display"));
                        services.AddSingleton<Display.IController, Display.Controller>();
                        services.AddOptions<Mqtt.Config>().ValidateDataAnnotations().Bind(hostContext.Configuration.GetSection("Mqtt"));
                        services.AddSingleton<Mqtt.IConnection, Mqtt.Connection>();
                        services.AddHostedService<Service>();
                    })
                .ConfigureLogging((hostContext, logging) => logging.AddConsole());
        }
    }
}

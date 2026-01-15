using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            using IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(o =>
                    {
                        o.SingleLine = true;
                        o.TimestampFormat = "HH:mm:ss ";
                    });

                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<AppRunner>();
                })
                .Build();

            host.Services.GetRequiredService<AppRunner>().Run();
        }
    }
}
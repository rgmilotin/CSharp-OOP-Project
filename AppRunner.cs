using Microsoft.Extensions.Logging;
using System;

namespace ConsoleApp5
{
    public sealed class AppRunner
    {
        private readonly ILogger<AppRunner> _logger;

        public AppRunner(ILogger<AppRunner> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Aplicatia porneste...");

            try
            {
                // codul tau existent, neschimbat:
                App.Run();

                _logger.LogInformation("Aplicatia s-a inchis normal.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare neasteptata (fatal) in aplicatie.");
                throw; // lasam exception-ul sa se vada ca sa nu ascundem problemele
            }
        }
    }
}
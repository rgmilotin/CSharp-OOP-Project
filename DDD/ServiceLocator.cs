using System;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp5
{
    public static class ServiceLocator
    {
        public static IServiceProvider? Provider { get; set; }

        public static T Get<T>() where T : notnull
        {
            if (Provider == null)
                throw new InvalidOperationException("ServiceLocator nu este inițializat.");

            return Provider.GetRequiredService<T>();
        }
    }
}
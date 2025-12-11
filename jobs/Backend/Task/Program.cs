using System;
using System.Collections.Generic;
using System.Linq;
using ExchangeRateUpdater.Cnb;
using Microsoft.Extensions.DependencyInjection;

namespace ExchangeRateUpdater
{
    public static class Program
    {
        private static IEnumerable<Currency> currencies = new[]
        {
            new Currency("USD"),
            new Currency("EUR"),
            new Currency("CZK"),
            new Currency("JPY"),
            new Currency("KES"),
            new Currency("RUB"),
            new Currency("THB"),
            new Currency("TRY"),
            new Currency("XYZ")
        };

        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            services.BuildServiceProvider().GetRequiredService<ApplicationRunner>().Run(currencies);
    
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register the IExchangeRateProvider implementation to use
            services.AddTransient<IExchangeRateProvider, CNBExchangeRateProvider>();
    
            services.AddTransient<ApplicationRunner>(); 
        }
        
        public class ApplicationRunner(IExchangeRateProvider provider)
        {
            public void Run(IEnumerable<Currency> currencies)
            {
                try
                {
                    var rates = provider.GetExchangeRates(currencies);

                    Console.WriteLine($"Successfully retrieved {rates.Count()} exchange rates:");
                    foreach (var rate in rates)
                    {
                        Console.WriteLine(rate.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Could not retrieve exchange rates: '{e.Message}'.");
                }
            }
        }
    }
}

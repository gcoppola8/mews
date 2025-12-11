# CNB Exchange Rate Provider

This project provides a robust and asynchronous mechanism to fetch and retrieve 
daily exchange rates from the Czech National Bank (CNB) official public data source.
It implements caching and a resilient retry mechanism to ensure reliable data 
retrieval. 

## Solution decisions

* ExchangeRateProvider is an Interface therefor has been renamed to IExchangeRateProvider
* To accomplish cleaner Dependency Injection I configured a ServiceCollection in the test program
* For better module organization, ExchangeRateUpdate.Cnb is a separate module containing the implementation for Cnb exchange rates
* Http fetching supports retry mechanism and caching

## Problems
One thing I don't like myself is that at the moment there is a circular dependency between ExchangeRateUpdater.Cnb and ExchangeRateUpdater, but that is only because the TestProgram is part of ExchangeRateUpdater.
In a production situation the consumer could import any implementation project, in this case Cnb, get also the ExchangeRateUpdater, and use the code similarly as written below.

## Usage

```
{
    private readonly IExchangeRateProvider _provider;

    public RateConsumer(IExchangeRateProvider provider)
    {
        _provider = provider;
    }

    public void GetCurrentRates()
    {
        // 1. Define the currencies you are interested in
        var requestedCurrencies = new List<Currency>
        {
            new Currency("EUR"),
            new Currency("USD"),
            new Currency("GBP")
        };

        // 2. Fetch the rates (synchronous wrapper)
        IEnumerable<ExchangeRate> rates = _provider.GetExchangeRates(requestedCurrencies);

        // 3. Process the results
        foreach (var rate in rates)
        {
            // Example: EUR -> CZK at 24.50
            Console.WriteLine($"1 {rate.TargetCurrency.Code} = {rate.Value} {rate.BaseCurrency.Code}");
        }
    }
}
```

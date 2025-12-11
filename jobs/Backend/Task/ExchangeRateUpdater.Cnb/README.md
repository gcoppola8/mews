# CNB Exchange Rate Provider

This project provides a robust and asynchronous mechanism to fetch and retrieve 
daily exchange rates from the Czech National Bank (CNB) official public data source.
It implements caching and a resilient retry mechanism to ensure reliable data 
retrieval. 

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

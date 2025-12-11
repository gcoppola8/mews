namespace ExchangeRateUpdater.Cnb;

public class CNBExchangeRateProvider : IExchangeRateProvider
{
    private static readonly Currency BaseCurrency = new Currency("CZK");

    private const string CNB_URL_FORMAT =
        "https://www.cnb.cz/en/financial-markets/foreign-exchange-market/central-bank-exchange-rate-fixing/central-bank-exchange-rate-fixing/daily.txt?date={0}";

    private static readonly HttpClient _httpClient = new HttpClient();

    private static (DateTime Date, IEnumerable<ExchangeRate> Rates)? _exchangeRateCache = null;

    public IEnumerable<ExchangeRate> GetExchangeRates(IEnumerable<Currency> currencies)
    {
        return GetExchangeRatesAsync(currencies).GetAwaiter().GetResult();
    }

    private async Task<IEnumerable<ExchangeRate>> GetExchangeRatesAsync(IEnumerable<Currency> currencies)
    {
        var today = DateTime.Today;

        // cache check
        if (_exchangeRateCache.HasValue && _exchangeRateCache.Value.Date == today)
        {
            // returns cached results
            return FilterRates(_exchangeRateCache.Value.Rates, currencies);
        }

        var allRates = await FetchAndParseRates(today);

        _exchangeRateCache = (today, allRates);

        return FilterRates(allRates, currencies);
    }

    private static IEnumerable<ExchangeRate> FilterRates(
        IEnumerable<ExchangeRate> allRates,
        IEnumerable<Currency> requestedCurrencies)
    {
        var requestedCodes =
            new HashSet<string>(requestedCurrencies.Select(c => c.Code), StringComparer.OrdinalIgnoreCase);

        return allRates
            .Where(rate => requestedCodes.Contains(rate.SourceCurrency.Code))
            .ToList();
    }

    private static async Task<IEnumerable<ExchangeRate>> FetchAndParseRates(DateTime date)
    {
        var dateString = date.ToString("dd.MM.yyyy");
        var url = string.Format(CNB_URL_FORMAT, dateString);
        var retry = 0;
        var maxRetry = 3;
        var initialDelay = TimeSpan.FromSeconds(5);

        string content = null;
        while (retry < maxRetry)
        {
            // log
            Console.WriteLine($"Fetching exchange rates for {dateString}: {url}, tentative {retry + 1}");

            try
            {
                content = await _httpClient.GetStringAsync(url);

                return Parse(content);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching exchange rates for {dateString}: {ex.Message}");

                retry++;

                if (retry >= maxRetry)
                {
                    Console.WriteLine($"Maximum retries ({maxRetry}) reached. Aborting fetch.");
                    return Enumerable.Empty<ExchangeRate>();
                }

                var delay = initialDelay.TotalMilliseconds * Math.Pow(2, retry - 1);

                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }
        
        return Enumerable.Empty<ExchangeRate>();
    }

    private static IEnumerable<ExchangeRate> Parse(string content)
    {
        var lines = content.Split('\n');
        if (lines.Length <= 2)
        {
            return Enumerable.Empty<ExchangeRate>();
        }

        // Skip first 2 lines with headers
        var dataLines = lines.Skip(2);

        var rates = new List<ExchangeRate>();

        foreach (var line in dataLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('|');
            if (parts.Length != 5) continue;

            // Columns are 0:Country, 1:Currency, 2:Amount, 3:Code, 4:Rate

            if (decimal.TryParse(parts[2], out decimal amount) && decimal.TryParse(parts[4], out decimal rateValue))
            {
                var targetCurrencyStr = parts[3].Trim();

                var ratePerUnit = rateValue / amount;

                var targetCurrency = new Currency(targetCurrencyStr);

                rates.Add(new ExchangeRate(targetCurrency, BaseCurrency, ratePerUnit));
            }
        }

        return rates;
    }
}
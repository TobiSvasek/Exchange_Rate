using System.Data.SqlClient;
using System.Globalization;

namespace Exchange_Rate_Sprint;

class Program
{
    static async Task Main(string[] args)
    {
        DateTime date = ProcessDate(args);

        if (date == default)
        {
            return;
        }

        date = GetAdjustedDate(date);

        double exchangeRate = await GetExchangeRate(date);

        using (SqlConnection connection =
               new SqlConnection(
                   "Server=stbechyn-sql.database.windows.net;Database=AdventureWorksDW2020;User Id=prvniit;Password=P@ssW0rd!;"))
        {
            connection.Open();

            using (SqlCommand command =
                   new SqlCommand("SELECT EnglishProductName, DealerPrice FROM DimProduct", connection))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter($"{date:yyyyMMdd}_adventureworks.csv"))
                    {
                        writer.WriteLine("Date;EnglishProductName;DealerPriceUSD;DealerPriceCZK");

                        while (reader.Read())
                        {
                            string productName = reader.GetString(0);
                            decimal priceUsd = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            decimal priceCzk = priceUsd * (decimal)exchangeRate;
                            writer.WriteLine($"{date:yyyy-MM-dd};{productName};{priceUsd};{priceCzk}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Při psaní do souboru nastala chyba: {ex.Message}");
                }
            }
        }
    }

    static DateTime ProcessDate(string[] args)
    {
        DateTime firstDate;
        DateTime nearestFriday;
        DateTime currentDate = DateTime.Now.Date;

        if (args.Length == 0)
        {
            firstDate = DateTime.Now;
        }
        else
        {
            try
            {
                firstDate = DateTime.ParseExact(args[0], "dd.MM.yyyy", CultureInfo.InvariantCulture);
            }
            catch (SystemException)
            {
                Console.WriteLine("Chyba: Neplatný formát data.");
                return default;
            }

            if (firstDate > DateTime.Now)
            {
                Console.WriteLine("Chyba: Datum je v budoucnu.");
                return default;
            }
        }

        if (firstDate.DayOfWeek == DayOfWeek.Saturday)
        {
            nearestFriday = firstDate.AddDays(-1);
        }
        else if (firstDate.DayOfWeek == DayOfWeek.Sunday)
        {
            nearestFriday = firstDate.AddDays(-2);
        }
        else if (currentDate.DayOfWeek == DayOfWeek.Monday)
        {
            nearestFriday = firstDate.AddDays(-3);
        }
        else if (currentDate.DayOfWeek == DayOfWeek.Tuesday)
        {
            nearestFriday = firstDate.AddDays(-4);
        }
        else
        {
            nearestFriday = firstDate;
        }

        Console.WriteLine($"Zadané datum: {firstDate:dd.MM.yyyy}");
        Console.WriteLine($"Výsledné datum: {nearestFriday:dd.MM.yyyy}");

        return nearestFriday;
    }

    static DateTime GetAdjustedDate(DateTime date)
    {
        DateTime currentDate = DateTime.Now.Date;

        if (date.Date == currentDate && date.DayOfWeek >= DayOfWeek.Wednesday && date.DayOfWeek <= DayOfWeek.Friday)
        {
            DateTime adjustedDate = date.AddDays(-1);
            Console.WriteLine("Pozor! Data jsou doopravdy ze dne zpět!");
            Console.WriteLine($"Reálné výsledné datum: {adjustedDate:dd.MM.yyyy}");
            return adjustedDate;
        }
        else
        {
            return date;
        }
    }

    static async Task<double> GetExchangeRate(DateTime exchangeDate)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string url =
                    $"https://www.cnb.cz/cs/financni-trhy/devizovy-trh/kurzy-devizoveho-trhu/kurzy-devizoveho-trhu/rok.txt?rok={exchangeDate.Year}";
                string response = await client.GetStringAsync(url);

                string[] lines = response.Split('\n');
                foreach (string line in lines)
                {
                    if (line.StartsWith(exchangeDate.ToString("dd.MM.yyyy")))
                    {
                        string[] parts = line.Split('|');
                        double exchangeRate = double.Parse(parts[29], CultureInfo.InvariantCulture) / 1000;
                        return exchangeRate;
                    }
                }
            }
            catch (Exception)
            {
                DateTime adjustedDate = GetAdjustedDate(exchangeDate.AddDays(-2));
                Console.WriteLine("Pozor! Data jsou doopravdy ze dvou dní zpět!");
                Console.WriteLine($"Reálné výsledné datum: {await GetExchangeRate(adjustedDate):dd.MM.yyyy}");
                return await GetExchangeRate(adjustedDate);
            }
        }

        throw new Exception("Exchange rate pro USD nenalezen.");
    }
}
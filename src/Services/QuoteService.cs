namespace Stakes.Services;

using Stakes.Models;


class QuoteService
{
    public static void AddQuote(string habit, string quote)
    {
        QuoteTable.Add(habit, quote);
        Console.WriteLine($"Added quote \"{quote}\" to habit \"{habit}\".");
    }

    public static void ListQuotes()
    {
        Console.WriteLine("Quotes:");
        foreach (Quote quote in QuoteTable.GetAll())
        {
            Console.WriteLine(
                $"{quote.Id}. {quote.HabitName, 8}\t\"{quote.Message}\""
            );
        }
    }

    public static void RandomQuote(string habit)
    {
        HabitTable.Get(habit); // error catching

        var quotes = new List<string>();
        foreach (Quote quote in QuoteTable.GetByHabit(habit))
        {
            quotes.Add(quote.Message);
        }

        if (quotes.Count == 0)
        {
            return;
        }

        var rnd = new Random();
        int r = rnd.Next(quotes.Count);
        Console.WriteLine(quotes[r]);
    }

    public static void RemoveQuotes(int[] quoteIds)
    {
        foreach (int quoteId in quoteIds)
        {
            QuoteTable.Get(quoteId); // error catching
            QuoteTable.Remove(quoteId);
            Console.WriteLine($"Unquoting quote: {quoteId}");
        }
    }
}

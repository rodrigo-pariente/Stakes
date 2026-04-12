class Actions
{
    public static void Report(List<string> habits, int progressBarSize)
    {
        Console.WriteLine(
            "Progress: `habit: [progress] [rate (effort-hours/day)] (streak)`"
        );
        foreach (string habitName in habits) // public string
        {
            try
            {
                HabitTable.Habit habit = HabitTable.Get(habitName);

                // habit.Lenght <= 8
                int emptySpaceSize = habitName.Length <= 8 ? 8 - habitName.Length : 0;
                var emptySpace = new string(' ', emptySpaceSize);
                Console.Write(habit.Name + emptySpace);

                // progressBar
                double hourGoal = habit.HourGoal;
                var time = HabitTrack.GetTotalHabitEffort(habitName); // this type is not explicit enough
                HabitTrack.PrintProgressBar(time, hourGoal, progressBarSize);

                // rate
                var rate = "~";
                try
                {
                    rate = HabitTrack.GetRate(habitName).ToString("0.##");
                }
                catch (IndexOutOfRangeException)
                {
                }

                Console.Write(" [" + rate + "]");

                // streak
                Console.Write(" (" + HabitTrack.GetStreak(habitName));
                Console.WriteLine(" days)");
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"ERROR: no habit of name {habitName}");
                continue;
            }
        }
    }

    public static void Progress(List<string> habits, int hourGoal, int progressBarSize)
    {
        Console.WriteLine("Progress: ");
        foreach(string habit in habits)
        {
            var time = HabitTrack.GetTotalHabitEffort(habit);
            Console.Write(habit + ":\t");
            HabitTrack.PrintProgressBar(time, hourGoal, progressBarSize);
        }
    }

    public static void Track(string[] habits, double hourGoal)
    {
        foreach (string habit in habits)
        {
            if (habit.Length > 8)
            {
                Console.WriteLine("ERROR: habit name must have at maximum 8 chars");
                continue;
            }
            Console.WriteLine("Started to track habit: " + habit);
            try
            {
                HabitTable.Add(habit, hourGoal);
            }
            catch (Microsoft.Data.Sqlite.SqliteException) // SOLVE THIS
            {
                Console.WriteLine("ERROR: habit names must be unique.");
                Environment.Exit(1);
            }
        }
    }

    public static void Commit(
        string habitName,
        string effortTime,
        string commitDate,
        string message
    )
    {
        try
        {
            CommitTable.Add(habitName, effortTime, commitDate, message);
            Console.WriteLine("Commited:");
            Console.WriteLine(commitDate + " I dedicated " + effortTime +
                " to " + habitName
            );
            Console.WriteLine("\"" + message + "\"");
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit match: {habitName}");
            Environment.Exit(1);
        }
    }

    public static void AddQuote(string habit, string quote)
    {
        try
        {
            QuoteTable.Add(habit, quote);
            Console.WriteLine("habit: " + habit);
            Console.WriteLine("quote: " + quote);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit match: {habit}");
            Environment.Exit(1);
        }
    }

    public static void Tracking()
    {
        Console.WriteLine("tracking the following habits:");
        foreach (var habit in HabitTable.GetAll())
        {
            var dedicatedTime = HabitTrack.GetTotalHabitEffort(habit.Name);
            Console.Write("- " + habit.Name + $"\t[{dedicatedTime}]");
            Console.WriteLine($"  ({habit.HourGoal})");
        }
    }

    public static void AllCommits()
    {
        Console.WriteLine("Commit History:");
        foreach (CommitTable.Commit commit in CommitTable.GetAll())
        {
            Console.WriteLine(
                commit.Id + "\t" + commit.CommitDate + ": " + commit.Message
            );
            Console.WriteLine("\t" + commit.EffortTime + " ~ " + commit.HabitName);
            Console.WriteLine();
        }
    }

    public static void QuotesOfHabit(string habit)
    {
        try
        {
            Console.WriteLine(habit + " Quotes:");
            foreach (QuoteTable.Quote quote in QuoteTable.GetOfHabit(habit))
            {
                Console.WriteLine(quote.Id + "\t" + habit + "\t" + quote.Message);
                Console.WriteLine();
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit of name {habit}");
            Environment.Exit(1);
        }
    }

    public static void AllQuotes()
    {
        Console.WriteLine("Quotes:");
        foreach (QuoteTable.Quote quote in QuoteTable.GetAll())
        {
            Console.WriteLine(quote.Id + "\t" + quote.HabitName + "\t" + quote.Message);
            Console.WriteLine();
        }
    }

    public static void RandomQuote(string habit)
    {
        try
        {
            var quotes = new List<string>();

            foreach (QuoteTable.Quote quote in QuoteTable.GetOfHabit(habit))
            {
                quotes.Add(quote.Message);
            }

            var rnd = new Random();
            int r = rnd.Next(quotes.Count);
            Console.WriteLine(quotes[r]);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit of name {habit}");
            Environment.Exit(1);
        }
    }

    public static void Update(
        int commitId,
        string habitName = "",
        string effortTime = "",
        string commitDate = "",
        string message = ""
    )
    {
        static void ChangedTo(string from, string to)
        {
            Console.WriteLine(from + " → " + to);
        };

        try
        {
            var commit = CommitTable.Get(commitId);
            CommitTable.Update(commitId, habitName, effortTime, commitDate, message);

            var changedCommit = CommitTable.Get(commitId);
            // Crimes against humanity:
            if (commit != changedCommit)
            {
                Console.WriteLine("Changed:");
            }

            if (commit.HabitName != changedCommit.HabitName)
            {
                ChangedTo(
                    commit.HabitName,
                    changedCommit.HabitName
                );
            }
            if (commit.EffortTime != changedCommit.EffortTime)
            {
                ChangedTo(commit.EffortTime.ToString(), changedCommit.EffortTime.ToString());
            }
            if (commit.CommitDate != changedCommit.CommitDate)
            {
                ChangedTo(commit.CommitDate.ToString(), changedCommit.CommitDate.ToString());
            }
            if (commit.Message != changedCommit.Message)
            {
                ChangedTo(commit.Message, changedCommit.Message);
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no commit of id {commitId}");
            if (! habitName.Equals(string.Empty))
            {
                Console.WriteLine($"ERROR: or habit of name \"{habitName}\"");
            }
            Environment.Exit(1);
        }

    }

    public static void Rename(string habit, string newName)
    {
        if (newName.Length > 8)
        {
            Console.WriteLine("ERROR: habit name must have at maximum 8 chars");
            Environment.Exit(1);
        }
        try
        {
            HabitTable.Get(habit); // error catching
            HabitTable.Rename(habit, newName);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new IndexOutOfRangeException("ERROR: no habit of given name");
        }
        Console.WriteLine("Renamed:");
        Console.WriteLine(habit + " → " + newName);
    }

    public static void UpdateHourGoal(string habit, double hourGoal)
    {
        double previousHourGoal = HabitTable.Get(habit).HourGoal;

        HabitTable.ChangeHourGoal(habit, hourGoal);

        Console.WriteLine("Updated:");
        Console.WriteLine("(" + habit + ") " + previousHourGoal+ " → " + hourGoal);
    }

    public static void Untrack(string[] habits)
    {
        foreach (string habit in habits)
        {
            try
            {
                HabitTable.Get(habit); // error catching
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"ERROR: no habit of given name \"{habit}\"");
                Environment.Exit(1);
            }
            Console.WriteLine("Untracking habit: " + habit);
            HabitTable.Remove(habit);
        }
    }

    public static void Uncommit(int[] commitIds)
    {
        foreach (int commitId in commitIds)
        {
            Console.WriteLine("Uncommiting commit: " + commitId);
            try
            {
                try
                {
                    CommitTable.Get(commitId); // error catching
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine($"ERROR: no commit of given id {commitId}");
                    Environment.Exit(1);
                }

                CommitTable.Remove(commitId);
            }
            catch(IndexOutOfRangeException)
            {
                Console.WriteLine($"ERROR: no commit of id {commitId}");
                Environment.Exit(1);
            }
        }
    }

    public static void Unquote(int[] quoteIds)
    {
        foreach (int quoteId in quoteIds)
        {
            Console.WriteLine("Unquoting quote: " + quoteId);
            try
            {
                QuoteTable.Get(quoteId); // error catching
                QuoteTable.Remove(quoteId);
            }
            catch(IndexOutOfRangeException)
            {
                Console.WriteLine($"ERROR: no quote of id {quoteId}");
                Environment.Exit(1);
            }
        }
    }
}

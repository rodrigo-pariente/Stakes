using System.Globalization;
using System.CommandLine;

class Program
{
    static void Main(string[] args)
    {
        Database.Initialize();

        // // rate <habit>
        // var rateHabit = new Argument<string[]>("habit"){
        //     Description = "habits to show rate of",
        // };
        // var rateCommand = new Command("rate");
        // rateCommand.Arguments.Add(rateHabit);
        // rateCommand.SetAction(parseResult =>
        // {
        //     string[] habits = parseResult.GetValue(rateHabit)!;
        //     Console.Write("Rate: ");
        //     foreach (string habit in habits)
        //     {
        //         Console.Write(HabitTrack.GetRate(HabitTable.GetId(habit)));
        //         Console.WriteLine(" effort-hours/day");
        //     }
        // });

        // // streak <habbit>
        // var streakHabit = new Argument<string[]>("habit"){
        //     Description = "habits to show streak of",
        // };
        // var streakCommand = new Command("streak");
        // streakCommand.Arguments.Add(streakHabit);
        // streakCommand.SetAction(parseResult =>
        // {
        //     string[] habits = parseResult.GetValue(streakHabit)!;
        //     Console.WriteLine("Streak:");
        //     foreach (string habit in habits)
        //     {
        //         Console.WriteLine(HabitTrack.GetStreak(HabitTable.GetId(habit)));
        //     }
        // });

        // report <habit>
        var reportHabit = new Argument<List<string>>("habit"){
            Description = "habits to show report of",
            DefaultValueFactory = parseResult =>
            {
                var habits = new List<string>();
                foreach(var habit in HabitTable.GetAll())
                {
                    habits.Add(habit.Name);
                }
                return habits;
            }
        };
        var reportProgressBarSize = new Option<int>("-s", "--size"){
            Description = "progress bar char size",
            DefaultValueFactory = parseResult => 35
        };
        var reportCommand = new Command("report");
        reportCommand.Arguments.Add(reportHabit);
        reportCommand.Options.Add(reportProgressBarSize);
        reportCommand.SetAction(parseResult =>
        {
            List<string> habits = parseResult.GetValue(reportHabit)!;
            int progressBarSize = parseResult.GetValue(reportProgressBarSize);
            Report(habits, progressBarSize);
        });


        // // bar <HH:MM:SS> [-g --hour-goal] [-s --size]
        // var barTime = new Argument<string>("time"){
        //     Description = "amount of time to make progress bar of",
        //     DefaultValueFactory = parseResult => string.Empty
        // };
        var barHourGoal = new Option<int>("-g", "--hour-goal"){
            Description = "progress bar base hour",
            DefaultValueFactory = parseResult => 12
        };
        var barProgressBarSize = new Option<int>("-s", "--size"){
            Description = "progress bar char size",
            DefaultValueFactory = parseResult => 35
        };
        // var barCommand = new Command("bar");
        // barCommand.Arguments.Add(barTime);
        // barCommand.Options.Add(barHourGoal);
        // barCommand.Options.Add(barProgressBarSize);
        // barCommand.SetAction(parseResult =>
        // {
        //     string rawTime = parseResult.GetValue(barTime)!;
        //     int hourGoal = parseResult.GetValue(barHourGoal);
        //     int progressBarSize = parseResult.GetValue(barProgressBarSize);
        //
        //     TimeSpan time;
        //     if (rawTime.Equals(string.Empty))
        //     {
        //         time = DateTime.Now.TimeOfDay;
        //     }
        //     else
        //     {
        //         string[] timeParts = rawTime.Split(':');
        //
        //         int hours, minutes, seconds;
        //         hours = int.Parse(timeParts[0]);
        //         minutes = timeParts.Length > 1 ? int.Parse(timeParts[1]) : 0;
        //         seconds = timeParts.Length > 2 ? int.Parse(timeParts[2]) : 0;
        //
        //         time = new TimeSpan(hours, minutes, seconds);
        //     }
        //     Console.WriteLine("Progress Bar:");
        //     HabitTrack.PrintProgressBar(time, hourGoal, progressBarSize);
        // });

        // // progress [HABIT] [-g --hour-goal] [-s --size]
        // var progressHabits = new Argument<List<string>>("habit"){
        //     Description = "habits to show progress bar of",
        //     DefaultValueFactory = parseResult =>
        //     {
        //         var habits = new List<string>();
        //         foreach(var habit in HabitTable.GetAll())
        //         {
        //             habits.Add(habit.Name);
        //         }
        //         return habits;
        //     }
        // };
        // var progressCommand = new Command("progress");
        // progressCommand.Arguments.Add(progressHabits);
        // progressCommand.Options.Add(barHourGoal);
        // progressCommand.Options.Add(barProgressBarSize);
        // progressCommand.SetAction(parseResult =>
        // {
        //     List<string> habits = parseResult.GetValue(progressHabits)!;
        //     int hourGoal = parseResult.GetValue(barHourGoal);
        //     int progressBarSize = parseResult.GetValue(barProgressBarSize);
        //
        //     Progress(habits, hourGoal, progressBarSize);
        // });

        // track <habit ...>
        var trackHabits = new Argument<string[]>("habits")
        {
            Description = "habits to start tracking",
            Arity = ArgumentArity.OneOrMore
        };

        var trackHourGoal = new Argument<double>("hour-goal")
        {
            Description = "habits to start tracking",
            DefaultValueFactory = parseResult => 1D
        };

        var trackCommand = new Command("track");
        trackCommand.Arguments.Add(trackHabits);
        trackCommand.Arguments.Add(trackHourGoal);

        trackCommand.SetAction(parseResult =>
        {
            string[] habits = parseResult.GetValue(trackHabits)!;
            double hourGoal = parseResult.GetValue(trackHourGoal);
            Track(habits, hourGoal);
        });


        // commit <habit> <effort-time>
        //     [-c --commit-date commit-time:=today] [-m message:=""]
        var commitHabit = new Argument<string>("habit"){
            Description = "dedicated habit"
        };
        var commitEffortTime = new Argument<string>("effort-time"){
            Description = "amount of time dedicated to habit",
        };
        var commitCommitTime = new Option<string>("--commit-date", "-c"){
            Description = "date of commitment to habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitMessage = new Option<string>("--message", "-m"){
            Description = "description of work done",
            DefaultValueFactory = parseResult => string.Empty
        };

        var commitCommand = new Command("commit");
        commitCommand.Arguments.Add(commitHabit);
        commitCommand.Arguments.Add(commitEffortTime);
        commitCommand.Options.Add(commitCommitTime);
        commitCommand.Options.Add(commitMessage);

        commitCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(commitHabit)!;
            TimeSpan effortTime = ParseTime(
                    parseResult.GetValue(commitEffortTime)!
            );
            string commitTime = parseResult.GetValue(commitCommitTime)!;

            try
            {
                commitTime = ToIso(commitTime);
            }
            catch (System.FormatException)
            {
                Console.WriteLine("ERROR: bad datetime format");
                Environment.Exit(1);
            }

            string message = parseResult.GetValue(commitMessage)!;
            Commit(habit, effortTime.ToString(), commitTime, message);
        });

        // quote <habit> <quote>
        var quoteHabit = new Argument<string>("habit"){
            Description = "habit the quote encourage"
        };
        var quoteQuote = new Argument<string>("quote"){
            Description = "encouraging cite"
        };

        var quoteCommand = new Command("quote");
        quoteCommand.Arguments.Add(quoteHabit);
        quoteCommand.Arguments.Add(quoteQuote);

        quoteCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(quoteHabit)!;
            string quote = parseResult.GetValue(quoteQuote)!;
            Quote(habit, quote);
        });

        // tracking
        var trackingCommand = new Command("tracking");
        trackingCommand.SetAction(parseResult => Tracking());

        // commits 
        var commitsCommand = new Command("commits");
        commitsCommand.SetAction(parseResult =>
        {
            AllCommits();
        });

        // quotes [habit]
        var quotesHabit = new Argument<string>("habit"){
            Description = "habit the quote encourage",
            DefaultValueFactory = parseResult => string.Empty
        };

        var quotesCommand = new Command("quotes");
        quotesCommand.Arguments.Add(quotesHabit);

        quotesCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(quotesHabit)!;
            if (habit.Equals(string.Empty))
            {
                AllQuotes();
            }
            else
            {
                QuotesOfHabit(habit);
            }
        });

        // courage [habit]
        var courageHabit = new Argument<string>("habit"){
            Description = "habit the quote encourage",
            DefaultValueFactory = parseResult =>
            {
                var habits = new List<string>();
                foreach (var habit in HabitTable.GetAll())
                {
                    habits.Add(habit.Name);
                }
                if (habits.Count == 0)
                {
                    return string.Empty;
                }
                var rnd = new Random();
                int r = rnd.Next(habits.Count);
                return habits[r];
            }
        };

        var courageCommand = new Command("courage");
        courageCommand.Arguments.Add(courageHabit);

        courageCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(courageHabit)!;
            if (habit.Equals(string.Empty))
            {
                Console.WriteLine("ERROR: habit not available");
            }
            RandomQuote(habit);
        });

        // rename <habit> <new-name>
        var renameHabit = new Argument<string>("habit"){
            Description = "habit to rename"
        };
        var renameNewName = new Argument<string>("new-name"){
            Description = "new name of habit"
        };

        var renameCommand = new Command("rename");
        renameCommand.Arguments.Add(renameHabit);
        renameCommand.Arguments.Add(renameNewName);

        renameCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(renameHabit)!;
            string newName = parseResult.GetValue(renameNewName)!;

            Rename(habit, newName);
        });

        // new-goal <habit> <new-hour-goal>
        var newGoalHabit = new Argument<string>("habit"){
            Description = "habit to update hour goal"
        };
        var newGoalGoal = new Argument<double>("new-hour-goal"){
            Description = "new hour goal"
        };

        var newGoalCommand = new Command("new-goal");
        newGoalCommand.Arguments.Add(newGoalHabit);
        newGoalCommand.Arguments.Add(newGoalGoal);

        newGoalCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(newGoalHabit)!;
            double newGoal = parseResult.GetValue(newGoalGoal);

            UpdateHourGoal(habit, newGoal);
        });

        // update <commit-id> ([-h --habit HABIT] [-e --effort-time TIME]
        // [-c --commit-time] [-m --message MESSAGE])
        var updateCommitId = new Argument<int>("commit-id"){
            Description = "id of commit to change"
        };
        var updateHabit = new Option<string>("-h", "--habit"){
            Description = "change commit's habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var updateEffortTime = new Option<string>("-e", "--effort-time"){
            Description = "change commit's effort-time",
            DefaultValueFactory = parseResult => string.Empty
        };
        var updateCommitTime = new Option<string>("-c", "--commit-date"){
            Description = "change commit's commit-date",
            DefaultValueFactory = parseResult => ToIso(DateTime.Now.ToString())
        };
        var updateMessage = new Option<string>("-m", "--message"){
            Description = "change commit's message",
            DefaultValueFactory = parseResult => string.Empty
        };

        var updateCommand = new Command("update");

        updateCommand.Arguments.Add(updateCommitId);
        updateCommand.Options.Add(updateHabit);
        updateCommand.Options.Add(updateEffortTime);
        updateCommand.Options.Add(updateCommitTime);
        updateCommand.Options.Add(updateMessage);

        updateCommand.SetAction(parseResult =>
        {
            int commitId = parseResult.GetValue(updateCommitId);
            string habit = parseResult.GetValue(updateHabit)!;
            TimeSpan effortTime = ParseTime(
                    parseResult.GetValue(commitEffortTime)!
            );

            string commitTime = parseResult.GetValue(updateCommitTime)!;
            try
            {
                commitTime = ToIso(commitTime);
            }
            catch (FormatException)
            {
                Console.WriteLine("ERROR: bad datetime format");
                Environment.Exit(1);
            }

            string message = parseResult.GetValue(updateMessage)!;
            Update(commitId, habit, effortTime.ToString(), commitTime, message);
        });

        // untrack <habit ...>
        var untrackHabits = new Argument<string[]>("habits")
        {
            Description = "habits to untrack",
            Arity = ArgumentArity.OneOrMore
        };

        var untrackCommand = new Command("untrack");
        untrackCommand.Arguments.Add(untrackHabits);

        untrackCommand.SetAction(parseResult =>
        {
            string[] habits = parseResult.GetValue(untrackHabits)!;
            Untrack(habits);
        });

        // uncommit <commit-id ...>
        var uncommitIds = new Argument<int[]>("commit-id")
        {
            Description = "id of commits to uncommit",
            Arity = ArgumentArity.OneOrMore
        };

        var uncommitCommand = new Command("uncommit");
        uncommitCommand.Arguments.Add(uncommitIds);

        uncommitCommand.SetAction(parseResult =>
        {
            int[] commitIds = parseResult.GetValue(uncommitIds)!;
            Uncommit(commitIds);
        });

        // unquote <quote-id ...>
        var unquoteIds = new Argument<int[]>("quote-id")
        {
            Description = "id of quotes to unquote",
            Arity = ArgumentArity.OneOrMore
        };

        var unquoteCommand = new Command("unquote");
        unquoteCommand.Arguments.Add(unquoteIds);

        unquoteCommand.SetAction(parseResult =>
        {
            int[] quoteIds = parseResult.GetValue(unquoteIds)!;
            Unquote(quoteIds);
        });


        // initialize root command
        var rootCommand = new RootCommand();

        /// add subcommands
        // C: create
        rootCommand.Subcommands.Add(trackCommand);
        rootCommand.Subcommands.Add(commitCommand);
        rootCommand.Subcommands.Add(quoteCommand);

        // R: read
        rootCommand.Subcommands.Add(trackingCommand);
        rootCommand.Subcommands.Add(commitsCommand);
        rootCommand.Subcommands.Add(quotesCommand);
        rootCommand.Subcommands.Add(courageCommand);
        rootCommand.Subcommands.Add(reportCommand);
        // rootCommand.Subcommands.Add(barCommand);
        // rootCommand.Subcommands.Add(progressCommand);
        // rootCommand.Subcommands.Add(rateCommand);
        // rootCommand.Subcommands.Add(streakCommand);

        // U: update
        rootCommand.Subcommands.Add(renameCommand);
        rootCommand.Subcommands.Add(updateCommand);
        rootCommand.Subcommands.Add(newGoalCommand);

        // D: delete
        rootCommand.Subcommands.Add(untrackCommand);
        rootCommand.Subcommands.Add(uncommitCommand);
        rootCommand.Subcommands.Add(unquoteCommand);


        // execute action
        rootCommand.Parse(args).Invoke();
    }

    static TimeSpan ParseTime(string time)
    {
        // "([HH..Hh][MM..Mmin][SS...Ss])"
        static void die()
        {
            Console.WriteLine("ERROR: bad format time");
            Environment.Exit(1);
        }

        int lastIdx = 0;

        int hIdx = time.IndexOf('h');
        uint hours = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] hIdx: {hIdx}");
        # endif
        if (hIdx != -1)
        {
            if (! uint.TryParse(time[0..hIdx], out hours))
            {
                die();
            }
            lastIdx = hIdx + 1;
        }

        int minIdx = time.IndexOf("min");
        uint minutes = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] hours: {hours}");
            Console.WriteLine($"[DEBUG] minIdx: {minIdx}");
        # endif
        if (minIdx != -1)
        {
            if (! uint.TryParse(time[(hIdx + 1)..minIdx], out minutes))
            {
                die();
            }
            lastIdx = minIdx + 3;
        }

        int sIdx = time.IndexOf('s');
        uint seconds = 0;
        # if DEBUG
            Console.WriteLine($"[DEBUG] minutes: {minutes}");
            Console.WriteLine($"[DEBUG] sIdx: {sIdx}");
        # endif
        if (sIdx != -1)
        {
            if (! uint.TryParse(time[lastIdx..sIdx], out seconds))
            {
                die();
            }
        }

        # if DEBUG
            Console.WriteLine($"[DEBUG] seconds: {seconds}");
        # endif

        if (hours == minutes && minutes == seconds && seconds == 0)
        {
            die();
        }

        return new TimeSpan(
            Convert.ToInt32(hours),
            Convert.ToInt32(minutes),
            Convert.ToInt32(seconds)
        );
    }

    // it needs iso to go and iso to come
    static string ToIso(string dateTime)
    {
        if (dateTime.Equals(string.Empty))
        {
            // default assigning this to commitCommitTime lead to
            // strange behaviour
            dateTime = DateTime.UtcNow.ToString();
        }
        var dt = DateTime.Parse(dateTime);
        var ut = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
        string isoDt = ut.ToString("s", CultureInfo.InvariantCulture); // + "Z";
        return isoDt;
    }

    static void Report(List<string> habits, int progressBarSize)
    {

        Console.WriteLine(
            "Progress: `habit: [progress] [rate (effort-hours/day)] (streak)`"
        );
        foreach (string habit in habits)
        {
            try
            {
                int habitId = HabitTable.GetId(habit);

                // habit Lenght <= 8
                int emptySpaceSize = habit.Length <= 8 ? 8 - habit.Length : 0;
                var emptySpace = new string(' ', emptySpaceSize);
                Console.Write(habit + emptySpace + ":");

                // progressBar
                double hourGoal = HabitTable.Get(habitId).HourGoal;
                var time = HabitTrack.GetTotalHabitEffort(HabitTable.GetId(habit));
                HabitTrack.PrintProgressBar(time, hourGoal, progressBarSize);

                // rate
                var rate = "~";
                try
                {
                    rate = HabitTrack.GetRate(habitId).ToString("0.##");
                }
                catch (IndexOutOfRangeException)
                {
                }

                Console.Write(" [" + rate + "]");

                Console.Write(" (" + HabitTrack.GetStreak(HabitTable.GetId(habit)));
                Console.WriteLine(" days)");
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"ERROR: no habit of name {habit}");
                continue;
            }
        }
    }

    static void Progress(List<string> habits, int hourGoal, int progressBarSize)
    {
        Console.WriteLine("Progress: ");
        foreach(string habit in habits)
        {
            var time = HabitTrack.GetTotalHabitEffort(HabitTable.GetId(habit));
            Console.Write(habit + ":\t");
            HabitTrack.PrintProgressBar(time, hourGoal, progressBarSize);
        }
    }

    static void Track(string[] habits, double hourGoal)
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
            catch (Microsoft.Data.Sqlite.SqliteException)
            {
                Console.WriteLine("ERROR: habit names must be unique.");
                Environment.Exit(1);
            }
        }
    }

    static void Commit(
        string habit,
        string effortTime,
        string commitTime,
        string message
    )
    {
        try
        {
            int habitId = HabitTable.GetId(habit);

            Console.WriteLine("Commited:");
            Console.WriteLine(commitTime + " I dedicated " + effortTime +
                " to " + habit
            );
            Console.WriteLine("\"" + message + "\"");
            CommitTable.Add(habitId, effortTime, commitTime, message);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit match: {habit}");
            Environment.Exit(1);
        }

    }

    static void Quote(string habit, string quote)
    {
        try
        {
            QuoteTable.Add(HabitTable.GetId(habit), quote);
            Console.WriteLine("habit: " + habit);
            Console.WriteLine("quote: " + quote);
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit match: {habit}");
            Environment.Exit(1);
        }
    }

    static void Tracking()
    {
        Console.WriteLine("tracking the following habits:");
        foreach (var habit in HabitTable.GetAll())
        {
            var dedicatedTime = HabitTrack.GetTotalHabitEffort(habit.Id);
            Console.Write("- " + habit.Name + $"\t[{dedicatedTime}]");
            Console.WriteLine($"  ({habit.HourGoal})");
        }
    }

    static void AllCommits()
    {
        Console.WriteLine("Commit History:");
        foreach (CommitTable.Commit commit in CommitTable.GetAll())
        {
            var habit = HabitTable.Get(commit.HabitId).Name;
            Console.WriteLine(
                commit.Id + "\t" + commit.CommitTime + ": " + commit.Message
            );
            Console.WriteLine("\t" + commit.EffortTime + " ~ " + habit);
            Console.WriteLine();
        }
    }

    static void QuotesOfHabit(string habit)
    {
        try
        {
            int habitId = HabitTable.GetId(habit);

            Console.WriteLine(habit + " Quotes:");
            foreach (QuoteTable.Quote quote in QuoteTable.GetOfHabit(habitId))
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

    static void AllQuotes()
    {
        string habit;

        Console.WriteLine("Quotes:");
        foreach (QuoteTable.Quote quote in QuoteTable.GetAll())
        {
            habit = HabitTable.Get(quote.HabitId).Name;
            Console.WriteLine(quote.Id + "\t" + habit + "\t" + quote.Message);
            Console.WriteLine();
        }
    }

    static void RandomQuote(string habit)
    {
        try
        {
            int habitId = HabitTable.GetId(habit);
            var quotes = new List<string>();

            foreach (QuoteTable.Quote quote in QuoteTable.GetOfHabit(habitId))
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

    static void Update(
        int commitId,
        string habit = "",
        string effortTime = "",
        string commitTime = "",
        string message = ""
    )
    {
        static void ChangedTo(string from, string to)
        {
            Console.WriteLine(from + " → " + to);
        };

        try
        {
            int habitId;
            if (habit.Equals(string.Empty))
            {
                habitId = -1;
            }
            else
            {
                habitId = HabitTable.GetId(habit);
            }
            var commit = CommitTable.Get(commitId);

            CommitTable.Update(commitId, habitId, effortTime, commitTime, message);
            var changedCommit = CommitTable.Get(commitId);

            // Crimes against humanity:
            if (commit != changedCommit)
            {
                Console.WriteLine("Changed:");
            }

            if (commit.HabitId != changedCommit.HabitId)
            {
                ChangedTo(
                    HabitTable.Get(commit.HabitId).Name,
                    HabitTable.Get(changedCommit.HabitId).Name
                );
            }
            if (commit.EffortTime != changedCommit.EffortTime)
            {
                ChangedTo(commit.EffortTime.ToString(), changedCommit.EffortTime.ToString());
            }
            if (commit.CommitTime != changedCommit.CommitTime)
            {
                ChangedTo(commit.CommitTime.ToString(), changedCommit.CommitTime.ToString());
            }
            if (commit.Message != changedCommit.Message)
            {
                ChangedTo(commit.Message, changedCommit.Message);
            }
        }
        catch (IndexOutOfRangeException)
        {
            Console.WriteLine($"ERROR: no habit of name {habit}");
            Environment.Exit(1);
        }

    }

    static void Rename(string habit, string newName)
    {
        if (newName.Length > 8)
        {
            Console.WriteLine("ERROR: habit name must have at maximum 8 chars");
            Environment.Exit(1);
        }
        HabitTable.Rename(habit, newName);
        Console.WriteLine("Renamed:");
        Console.WriteLine(habit + " → " + newName);
    }

    static void UpdateHourGoal(string habit, double hourGoal)
    {
        HabitTable.ChangeHourGoal(habit, hourGoal);
        Console.WriteLine("Updated:");
        Console.WriteLine(habit + ": " + hourGoal);
    }

    static void Untrack(string[] habits)
    {
        foreach (string habit in habits)
        {
            Console.WriteLine("Untracking habit: " + habit);
            HabitTable.Remove(habit);
        }
    }

    static void Uncommit(int[] commitIds)
    {
        foreach (int commitId in commitIds)
        {
            Console.WriteLine("Uncommiting commit: " + commitId);
            CommitTable.Remove(commitId);
        }
    }

    static void Unquote(int[] quoteIds)
    {
        foreach (int quoteId in quoteIds)
        {
            Console.WriteLine("Unquoting quote: " + quoteId);
            QuoteTable.Remove(quoteId);
        }
    }
}

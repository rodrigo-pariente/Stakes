using System.CommandLine;


class Program
{
    static void Main(string[] args)
    {
        // load configuration
        var config = Config.Load();

        // create configuration if does not exist
        if (!File.Exists(Config.configurationPath))
        {
            Config.Create();
        }

        // initialize the database
        Database.Initialize(config.DatabasePath);

        // Habit
        /// habit add <name ...>
        var habitAddHabits = new Argument<string[]>("habit"){
            Description = "habits to start tracking",
        };
        var habitAddGoal = new Argument<double>("hour-goal")
        {
            Description = "daily time goal in hours",
            DefaultValueFactory = parseResult => 1D
        };

        var habitAddCommand = new Command("add");
        habitAddCommand.Arguments.Add(habitAddHabits);
        habitAddCommand.Arguments.Add(habitAddGoal);

        habitAddCommand.SetAction(parseResult =>
        {
            var habits = parseResult.GetValue(habitAddHabits)!;
            var goal = parseResult.GetValue(habitAddGoal);
            Actions.Track(habits, goal);
        });

        /// habit list
        var habitListCommand = new Command("list");
        habitListCommand.SetAction(parseResult => Actions.Tracking());

        /// habit update <habit> [-n --name name] [-g --goal time]
        var habitUpdateHabit = new Argument<string>("habit"){
            Description = "habit to habitUpdate"
        };
        var habitUpdateName = new Option<string>("--name", "-n"){
            Description = "new name of habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var habitUpdateGoal = new Option<double>("--goal", "-g"){
            Description = "daily goal time in hours",
            DefaultValueFactory = parseResult => -1D
        };

        var habitUpdateCommand = new Command("update");
        habitUpdateCommand.Arguments.Add(habitUpdateHabit);
        habitUpdateCommand.Options.Add(habitUpdateName);
        habitUpdateCommand.Options.Add(habitUpdateGoal);

        habitUpdateCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(habitUpdateHabit)!;
            string newName = parseResult.GetValue(habitUpdateName)!;
            double goal = parseResult.GetValue(habitUpdateGoal)!;

            try
            {
                if (goal >= 0)
                {
                    Actions.UpdateHourGoal(habit, goal);
                }

                if (!newName.Equals(string.Empty))
                {
                    Actions.Rename(habit, newName);
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("ERROR: no habit of given name");
                Environment.Exit(1);
            }
        });

        /// habit remove <habit ...>
        var habitRemoveHabits = new Argument<string[]>("habits")
        {
            Description = "habits to untrack",
            Arity = ArgumentArity.OneOrMore
        };

        var habitRemoveCommand = new Command("remove");
        habitRemoveCommand.Aliases.Add("rm");
        habitRemoveCommand.Arguments.Add(habitRemoveHabits);

        habitRemoveCommand.SetAction(parseResult =>
        {
            string[] habits = parseResult.GetValue(habitRemoveHabits)!;
            Actions.Untrack(habits);
        });

        var habitCommand = new Command("habit");
        habitCommand.Subcommands.Add(habitAddCommand);
        habitCommand.Subcommands.Add(habitListCommand);
        habitCommand.Subcommands.Add(habitUpdateCommand);
        habitCommand.Subcommands.Add(habitRemoveCommand);


        // Quote
        /// quote add <habit> <quote>
        var quoteAddHabit = new Argument<string>("habit"){
            Description = "habit the quote encourage"
        };
        var quoteAddQuote = new Argument<string>("quote"){
            Description = "encouraging cite"
        };

        var quoteAddCommand = new Command("add");
        quoteAddCommand.Arguments.Add(quoteAddHabit);
        quoteAddCommand.Arguments.Add(quoteAddQuote);

        quoteAddCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(quoteAddHabit)!;
            string quote = parseResult.GetValue(quoteAddQuote)!;
            Actions.AddQuote(habit, quote);
        });

        var quoteListCommand = new Command("list");
        quoteListCommand.SetAction(parseResult => Actions.AllQuotes());

        /// quote remove <quote-id ...>
        var quoteRemoveIds = new Argument<int[]>("id")
        {
            Description = "id of quotes to remove",
            Arity = ArgumentArity.OneOrMore
        };

        var quoteRemoveCommand = new Command("remove");
        quoteRemoveCommand.Aliases.Add("rm");
        quoteRemoveCommand.Arguments.Add(quoteRemoveIds);

        quoteRemoveCommand.SetAction(parseResult =>
        {
            int[] quoteIds = parseResult.GetValue(quoteRemoveIds)!;
            Actions.Unquote(quoteIds);
        });

        var quoteCommand = new Command("quote");
        quoteCommand.Subcommands.Add(quoteAddCommand);
        quoteCommand.Subcommands.Add(quoteListCommand);
        quoteCommand.Subcommands.Add(quoteRemoveCommand);


        // Commits
        ///  commit add <habit> <effort-time>
        ///    [-d --date date:=todays date] [-m message:=""]
        var commitAddHabit = new Argument<string>("habit"){
            Description = "dedicated habit"
        };
        var commitAddEffortTime = new Argument<string>("effort-time"){
            Description = "amount of time dedicated to habit",
        };
        var commitAddDate = new Option<string>("--date", "-d"){
            Description = "date of commitment to habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitAddMessage = new Option<string>("--message", "-m"){
            Description = "description of work done",
            DefaultValueFactory = parseResult => string.Empty
        };

        var commitAddCommand = new Command("add");
        commitAddCommand.Arguments.Add(commitAddHabit);
        commitAddCommand.Arguments.Add(commitAddEffortTime);
        commitAddCommand.Options.Add(commitAddDate);
        commitAddCommand.Options.Add(commitAddMessage);

        commitAddCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(commitAddHabit)!;
            TimeSpan effortTime;
            try
            {
                effortTime = Parse.ParseTime(
                        parseResult.GetValue(commitAddEffortTime)!
                );

                string commitDate = parseResult.GetValue(commitAddDate)!;

                try
                {
                    commitDate = Parse.ToIso(commitDate);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                string message = parseResult.GetValue(commitAddMessage)!;
                Actions.Commit(habit, effortTime.ToString(), commitDate, message);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        });

        /// commit list
        var commitListCommand = new Command("list");
        commitListCommand.SetAction(parseResult => Actions.AllCommits());

        /// commit remove <commit-id ...>
        var commitRemoveIds = new Argument<int[]>("id")
        {
            Description = "id of commits to remove",
            Arity = ArgumentArity.OneOrMore
        };

        /// commit update <commit-id> [-h --habit HABIT]
        ///   [-e --effort-time TIME] [-c --commit-time] [-m --message MESSAGE]
        var commitUpdateCommitId = new Argument<int>("commit-id"){
            Description = "id of commit to change"
        };
        var commitUpdateHabit = new Option<string>("-h", "--habit"){
            Description = "change commit's habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateEffortTime = new Option<string>("-e", "--effort-time"){
            Description = "change commit's effort-time",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateCommitDate = new Option<string>("-d", "--date"){
            Description = "change commit's date",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateMessage = new Option<string>("-m", "--message"){
            Description = "change commit's message",
            DefaultValueFactory = parseResult => string.Empty
        };

        var commitUpdateCommand = new Command("update");

        commitUpdateCommand.Arguments.Add(commitUpdateCommitId);
        commitUpdateCommand.Options.Add(commitUpdateHabit);
        commitUpdateCommand.Options.Add(commitUpdateEffortTime);
        commitUpdateCommand.Options.Add(commitUpdateCommitDate);
        commitUpdateCommand.Options.Add(commitUpdateMessage);

        commitUpdateCommand.SetAction(parseResult =>
        {
            int commitId = parseResult.GetValue(commitUpdateCommitId);
            string habit = parseResult.GetValue(commitUpdateHabit)!;

            string effortTime = parseResult.GetValue(commitUpdateEffortTime)!;
            if (! effortTime.Equals(string.Empty))
            {
                try
                {
                    effortTime = Parse.ParseTime(effortTime).ToString();
                }
                catch (FormatException)
                {
                    Console.WriteLine("ERROR: bad time format");
                    Environment.Exit(1);
                }
            }

            string commitDate = parseResult.GetValue(commitUpdateCommitDate)!;
            if (! commitDate.Equals(string.Empty))
            {
                try
                {
                    commitDate = Parse.ToIso(commitDate);
                }
                catch (FormatException)
                {
                    Console.WriteLine("ERROR: bad datetime format");
                    Environment.Exit(1);
                }
            }

            string message = parseResult.GetValue(commitUpdateMessage)!;
            Actions.Update(commitId, habit, effortTime, commitDate, message);
        });

        var commitRemoveCommand = new Command("remove");
        commitRemoveCommand.Aliases.Add("rm");
        commitRemoveCommand.Arguments.Add(commitRemoveIds);

        commitRemoveCommand.SetAction(parseResult =>
        {
            int[] commitIds = parseResult.GetValue(commitRemoveIds)!;
            Actions.Uncommit(commitIds);
        });

        var commitCommand = new Command("commit");
        commitCommand.Subcommands.Add(commitAddCommand);
        commitCommand.Subcommands.Add(commitListCommand);
        commitCommand.Subcommands.Add(commitUpdateCommand);
        commitCommand.Subcommands.Add(commitRemoveCommand);

        // Habit Tracking Utilities
        /// report <habit>
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
            Actions.Report(habits, progressBarSize);
        });

        /// courage [habit := all]
        var courageHabit = new Argument<string>("habit"){
            Description = "habit the quote encourage",
            DefaultValueFactory = parseResult =>
            {
                // this can be simplified
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
            Actions.RandomQuote(habit);
        });


        // Aliases
        /// track <habit ...>
        var trackCommand = new Command("track", "alias for habit add");

        trackCommand.Arguments.Add(habitAddHabits);
        trackCommand.Arguments.Add(habitAddGoal);

        trackCommand.SetAction(parseResult =>
        {
            string[] habits = parseResult.GetValue(habitAddHabits)!;
            double goal = parseResult.GetValue(habitAddGoal);
            Actions.Track(habits, goal);
        });

        /// tracking
        var trackingCommand = new Command("tracking", "alias for habit list");
        trackingCommand.SetAction(parseResult => Actions.Tracking());

        /// untrack <habit ...>
        var untrackCommand = new Command("untrack", "alias for habit remove");
        untrackCommand.Arguments.Add(habitRemoveHabits);

        untrackCommand.SetAction(parseResult =>
        {
            string[] habits = parseResult.GetValue(habitRemoveHabits)!;
            Actions.Untrack(habits);
        });

        /// log <habit> <effort-time> [-m --message TEXT] [-d --date DATE]
        var logCommand = new Command("log", "alias for commit add");

        logCommand.Arguments.Add(commitAddHabit);
        logCommand.Arguments.Add(commitAddEffortTime);
        logCommand.Options.Add(commitAddDate);
        logCommand.Options.Add(commitAddMessage);

        logCommand.SetAction(parseResult =>
        {
            string habit = parseResult.GetValue(commitAddHabit)!;
            TimeSpan effortTime;
            try
            {
                effortTime = Parse.ParseTime(
                        parseResult.GetValue(commitAddEffortTime)!
                );

                string commitDate = parseResult.GetValue(commitAddDate)!;

                try
                {
                    commitDate = Parse.ToIso(commitDate);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }

                string message = parseResult.GetValue(commitAddMessage)!;
                Actions.Commit(habit, effortTime.ToString(), commitDate, message);
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
        });


        // initialize root command
        var rootCommand = new RootCommand();

        // add subcommands
        /// CRUD
        rootCommand.Subcommands.Add(habitCommand);
        rootCommand.Subcommands.Add(commitCommand);
        rootCommand.Subcommands.Add(quoteCommand);

        /// Habit Tracking Utilities
        rootCommand.Subcommands.Add(courageCommand);
        rootCommand.Subcommands.Add(reportCommand);

        /// Aliases
        rootCommand.Subcommands.Add(trackCommand);
        rootCommand.Subcommands.Add(trackingCommand);
        rootCommand.Subcommands.Add(untrackCommand);
        rootCommand.Subcommands.Add(logCommand);

        /// execute action
        rootCommand.Parse(args).Invoke();
    }
}

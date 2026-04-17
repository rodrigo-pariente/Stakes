// TO-DO: Move some arguments validation into Validators

using System.CommandLine;
using Pastel;
using Stakes.Core;
using Stakes.Services;


class Program
{
    // Pretty print of errors instead of crash
    private static void SafeRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exceptions.StakesException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error.");
            Console.Error.WriteLine(ex.Message);
            Environment.Exit(2);
        }
    }

    static int Main(string[] args)
    {
        // Load configuration
        var config = Config.Load();

        // Initialize Stakes application folder
        SafeRun(() =>
            InitializeApplication.InitializeApplicationFolder(config)
        );


        // Habit commands
        // habit add <name ...> [-h --hour-goal HOUR] [-p --per-day DAY]
        var habitAddHabits = new Argument<string[]>("habit"){
            Description = "Habits to start tracking",
        };
        var habitAddHourGoal = new Option<double>("-h", "--hour-goal")
        {
            Description = "Time goal in hours",
            DefaultValueFactory = parseResult => config.HourGoal
        };
        habitAddHourGoal.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(habitAddHourGoal) < 0)
            {
                parseResult.AddError("Must be non-negative.");
            }
        });
        var habitAddPerDayGoal = new Option<int>("-p", "--per-day")
        {
            Description = "Amount of days in which the time goal is based",
            DefaultValueFactory = parseResult => config.PerDayGoal
        };
        habitAddPerDayGoal.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(habitAddPerDayGoal) < 0)
            {
                parseResult.AddError("Must be non-negative.");
            }
        });

        var habitAddCommand = new Command(
            "add",
            "Add habit to start tracking"
        );
        habitAddCommand.Arguments.Add(habitAddHabits);
        habitAddCommand.Options.Add(habitAddHourGoal);
        habitAddCommand.Options.Add(habitAddPerDayGoal);

        habitAddCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                var habits = parseResult.GetValue(habitAddHabits)!;
                var hourGoal = parseResult.GetValue(habitAddHourGoal);
                var perDayGoal = parseResult.GetValue(habitAddPerDayGoal);
                HabitService.AddHabit(habits, hourGoal, perDayGoal);
            });
        });

        // habit list
        var habitListCommand = new Command("list", "List tracking habits");
        habitListCommand.SetAction(parseResult =>
        {
            SafeRun(() => HabitService.ListHabits());
        });

        // habit update <habit> [-n --name name] [-h --hour-goal time]
        var habitUpdateHabit = new Argument<string>("habit"){
            Description = "Habit to habitUpdate"
        };
        var habitUpdateName = new Option<string>("--name", "-n"){
            Description = "New name of habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var habitUpdateHourGoal = new Option<double?>("--hour-goal", "-h"){
            Description = "Daily goal time in hours",
            DefaultValueFactory = parseResult => null
        };
        habitUpdateHourGoal.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(habitUpdateHourGoal) < 0)
            {
                parseResult.AddError("Must be non-negative.");
            }
        });
        var habitUpdatePerDayGoal = new Option<int?>("--per-day", "-p"){
            Description = "Amount of days in which the time goal is based",
            DefaultValueFactory = parseResult => null
        };
        habitUpdatePerDayGoal.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(habitUpdatePerDayGoal) < 0)
            {
                parseResult.AddError("Must be non-negative.");
            }
        });

        var habitUpdateCommand = new Command(
            "update",
            "Update habit parameters"
        );
        habitUpdateCommand.Arguments.Add(habitUpdateHabit);
        habitUpdateCommand.Options.Add(habitUpdateName);
        habitUpdateCommand.Options.Add(habitUpdateHourGoal);
        habitUpdateCommand.Options.Add(habitUpdatePerDayGoal);

        habitUpdateCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string habit = parseResult.GetValue(habitUpdateHabit)!;
                string newName = parseResult.GetValue(habitUpdateName)!;
                double? hourGoal = parseResult.GetValue(habitUpdateHourGoal);
                int? perDayGoal = parseResult.GetValue(habitUpdatePerDayGoal);

                HabitService.UpdateHabit(
                    habit,
                    newName,
                    hourGoal,
                    perDayGoal
                );
            });
        });

        // habit remove <habit ...>
        var habitRemoveHabits = new Argument<string[]>("habits")
        {
            Description = "Habits to untrack",
            Arity = ArgumentArity.OneOrMore
        };

        var habitRemoveCommand = new Command(
            "remove",
            "Stop tracking habit"
        );
        habitRemoveCommand.Aliases.Add("rm");
        habitRemoveCommand.Arguments.Add(habitRemoveHabits);

        habitRemoveCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string[] habits = parseResult.GetValue(habitRemoveHabits)!;

                HabitService.RemoveHabits(habits);
            });
        });

        var habitCommand = new Command("habit", "Habit commands");
        habitCommand.Subcommands.Add(habitAddCommand);
        habitCommand.Subcommands.Add(habitListCommand);
        habitCommand.Subcommands.Add(habitUpdateCommand);
        habitCommand.Subcommands.Add(habitRemoveCommand);


        // Quote commands
        // quote add <habit> <quote>
        var quoteAddHabit = new Argument<string>("habit"){
            Description = "Habit the quote encourage"
        };
        var quoteAddQuote = new Argument<string>("quote"){
            Description = "Encouraging cite"
        };

        var quoteAddCommand = new Command("add", "Add quote to habit");
        quoteAddCommand.Arguments.Add(quoteAddHabit);
        quoteAddCommand.Arguments.Add(quoteAddQuote);

        quoteAddCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string habit = parseResult.GetValue(quoteAddHabit)!;
                string quote = parseResult.GetValue(quoteAddQuote)!;

                QuoteService.AddQuote(habit, quote);
            });
        });

        var quoteListCommand = new Command("list", "List quotes");
        quoteListCommand.SetAction(parseResult => QuoteService.ListQuotes());

        // quote remove <quote-id ...>
        var quoteRemoveIds = new Argument<int[]>("id")
        {
            Description = "Id of quotes to remove",
            Arity = ArgumentArity.OneOrMore
        };

        var quoteRemoveCommand = new Command(
            "remove",
            "Remove quote from habit"
        );
        quoteRemoveCommand.Aliases.Add("rm");
        quoteRemoveCommand.Arguments.Add(quoteRemoveIds);

        quoteRemoveCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                int[] quoteIds = parseResult.GetValue(quoteRemoveIds)!;

                QuoteService.RemoveQuotes(quoteIds);
            });
        });

        var quoteCommand = new Command("quote", "Quote commands");
        quoteCommand.Subcommands.Add(quoteAddCommand);
        quoteCommand.Subcommands.Add(quoteListCommand);
        quoteCommand.Subcommands.Add(quoteRemoveCommand);


        // Commit commands
        //  commit add <habit> <time>
        //    [-d --date date:=todays date] [-m message:=""]
        var commitAddHabit = new Argument<string>("habit"){
            Description = "Dedicated habit"
        };
        var commitAddTime = new Argument<string>("time"){
            Description = "Amount of time dedicated to habit",
        };
        var commitAddDate = new Option<string>("--date", "-d"){
            Description = "Date of commitment to habit",
            DefaultValueFactory = parseResult => "today"
        };
        commitAddDate.Validators.Add(parseResult =>
        {
            string date = parseResult.GetValue(commitAddDate)!;
            if (date.Equals("today"))
            {
                return;
            }
            try
            {
                DateTime.Parse(date);
            }
            catch(FormatException)
            {
                parseResult.AddError("Invalid datetime format");
            }
        });
        var commitAddMessage = new Option<string>("--message", "-m"){
            Description = "Description of work done",
            DefaultValueFactory = parseResult => string.Empty
        };

        var commitAddCommand = new Command("add", "Log effort to habit");
        commitAddCommand.Arguments.Add(commitAddHabit);
        commitAddCommand.Arguments.Add(commitAddTime);
        commitAddCommand.Options.Add(commitAddDate);
        commitAddCommand.Options.Add(commitAddMessage);

        commitAddCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string habit = parseResult.GetValue(commitAddHabit)!;
                string time = TimeParser.ParseTime(
                    parseResult.GetValue(commitAddTime)!
                ).ToString();
                string date = parseResult.GetValue(commitAddDate)!;
                if (date.Equals("today"))
                {
                    date = DateTime.Now.ToString();
                }
                string message = parseResult.GetValue(commitAddMessage)!;

                CommitService.AddCommit(
                    habit,
                    time,
                    date,
                    message
                );
            });
        });

        // commit list
        var commitListCommand = new Command("list", "List commits");
        commitListCommand.SetAction(parseResult =>
        {
            CommitService.ListCommits();
        });

        // commit remove <commit-id ...>
        var commitRemoveIds = new Argument<int[]>("id")
        {
            Description = "Id of commits to remove",
            Arity = ArgumentArity.OneOrMore
        };

        // commit update <commit-id> [-h --habit HABIT]
        //   [-t --time TIME] [-c --commit-time] [-m --message MESSAGE]
        var commitUpdateCommitId = new Argument<int>("commit-id"){
            Description = "Id of commit to change"
        };
        var commitUpdateHabit = new Option<string>("-h", "--habit"){
            Description = "Change commit's habit",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateTime = new Option<string>("-t", "--time"){
            Description = "Change commit's time",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateDate = new Option<string>("-d", "--date"){
            Description = "Change commit's date",
            DefaultValueFactory = parseResult => string.Empty
        };
        var commitUpdateMessage = new Option<string>("-m", "--message"){
            Description = "Change commit's message",
            DefaultValueFactory = parseResult => string.Empty
        };

        var commitUpdateCommand = new Command(
            "update",
            "Update commit parameters"
        );

        commitUpdateCommand.Arguments.Add(commitUpdateCommitId);
        commitUpdateCommand.Options.Add(commitUpdateHabit);
        commitUpdateCommand.Options.Add(commitUpdateTime);
        commitUpdateCommand.Options.Add(commitUpdateDate);
        commitUpdateCommand.Options.Add(commitUpdateMessage);

        commitUpdateCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                int commitId = parseResult.GetValue(commitUpdateCommitId);
                string habit = parseResult.GetValue(commitUpdateHabit)!;

                string time = parseResult.GetValue(commitUpdateTime)!;
                if (!time.Equals(string.Empty))
                {
                    time = TimeParser.ParseTime(time).ToString();
                }

                string date = parseResult.GetValue(commitUpdateDate)!;
                if (!date.Equals(string.Empty))
                {
                    try
                    {
                        date = DateTime.Parse(date).ToString();
                    }
                    catch(FormatException)
                    {
                        throw new Exceptions.DateTimeFormatException();
                    }
                }

                string message = parseResult.GetValue(commitUpdateMessage)!;

                CommitService.UpdateCommit(
                    commitId,
                    habit,
                    time,
                    date,
                    message
                );
            });
        });

        var commitRemoveCommand = new Command(
            "remove",
            "Remove commit from habit"
        );
        commitRemoveCommand.Aliases.Add("rm");
        commitRemoveCommand.Arguments.Add(commitRemoveIds);

        commitRemoveCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                int[] commitIds = parseResult.GetValue(commitRemoveIds)!;

                CommitService.RemoveCommits(commitIds);
            });
        });

        var commitCommand = new Command("commit", "Commit commands");
        commitCommand.Subcommands.Add(commitAddCommand);
        commitCommand.Subcommands.Add(commitListCommand);
        commitCommand.Subcommands.Add(commitUpdateCommand);
        commitCommand.Subcommands.Add(commitRemoveCommand);


        // Habit tracking commands
        // report <habit, ...> [-c --color COLORSTRING]
        //   [--style STYLESTRING] [-p --progress-char CHAR]
        //   [-P --alt-progress-char CHAR] [-e --progress-empty-char CHAR]
        var reportHabit = new Argument<string[]>("habit"){
            Description = "Habits to show report of",
            DefaultValueFactory = parseResult => ["all-habits"]
        };
        var reportProgressBarSize = new Option<int>("--size"){
            Description = "Progress bar size",
            DefaultValueFactory = parseResult => 35
        };
        reportProgressBarSize.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(reportProgressBarSize) < 5)
            {
                parseResult.AddError(
                    "Progress bar size must be 5 or higher chars long."
                );
            }
        });
        var reportProgressChar = new Option<string>("-p", "--progress-char"){
            Description = "Progress bar char",
            DefaultValueFactory = parseResult => config.ProgressChar.ToString()
        };
        reportProgressChar.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(reportProgressChar)!.Length != 1)
            {
                parseResult.AddError("Must be a single char.");
            }
        });
        var reportAltProgressChar = new Option<string>(
            "-P", "--alt-progress-char"
        ){
            Description = "Progress bar secondary char",
            DefaultValueFactory = parseResult =>
        {
            return config.AltProgressChar.ToString();
        }};
        reportAltProgressChar.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(reportAltProgressChar)!.Length != 1)
            {
                parseResult.AddError("Must be a single char.");
            }
        });
        var reportProgressEmptyChar = new Option<string>(
            "-e", "--progress-empty-char"
        ){
            Description = "Progress bar empty char",
            DefaultValueFactory = parseResult =>
        {
            return config.EmptyProgressChar.ToString();
        }};
        reportProgressEmptyChar.Validators.Add(parseResult =>
        {
            if (parseResult.GetValue(reportProgressEmptyChar)!.Length != 1)
            {
                parseResult.AddError("Must be a single char.");
            }
        });
        var reportColorsDescription =
            "Progress bar HEX colors\n Format: \"HEX,HEX,...\"\n";
        var reportColors = new Option<string>("-c", "--colors"){
            Description = reportColorsDescription,
            DefaultValueFactory = parseResult => config.ProgressColors
        };
        var reportStyleDescription =
            "Report style string, show or hide components.\n" +
            " Format: \"+component,-component,...\".\n" +
            " Operators:\n  +: Show component.\n  -: Hide component.\n";
        var reportStyle = new Option<string>("--style"){
            Description = reportStyleDescription,
            DefaultValueFactory = parseResult => config.ReportStyle
        };

        var reportCommand = new Command(
            "report",
            "Show habit tracking metrics"
        );
        reportCommand.Arguments.Add(reportHabit);
        reportCommand.Options.Add(reportProgressBarSize);
        reportCommand.Options.Add(reportProgressChar);
        reportCommand.Options.Add(reportAltProgressChar);
        reportCommand.Options.Add(reportProgressEmptyChar);
        reportCommand.Options.Add(reportColors);
        reportCommand.Options.Add(reportStyle);
        reportCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string[] habits = parseResult.GetValue(reportHabit)!;

                int barSize = parseResult.GetValue(reportProgressBarSize);

                char progressChar = parseResult.GetValue(
                    reportProgressChar
                )![0];
                char altProgressChar = parseResult.GetValue(
                    reportAltProgressChar
                )![0];
                char progressEmptyChar = parseResult.GetValue(
                    reportProgressEmptyChar
                )![0];

                string colorsString = parseResult.GetValue(reportColors)!;
                string[] colors = colorsString.Split(",");
                foreach(var color in colors)
                {
                    try
                    {
                        "".Pastel(color);
                    }
                    catch(ArgumentException)
                    {
                        throw new Exceptions.ColorStringException();
                    }
                }

                ProgressBar.Initialize(
                    progressEmptyChar,
                    progressChar,
                    altProgressChar,
                    barSize,
                    colors
                );

                string style = parseResult.GetValue(reportStyle)!;

                ReportService.Report(habits, style);
            });
        });

        // courage <habit>
        var courageHabit = new Argument<string>("habit"){
            Description = "Habit the quote encourage"
        };
        var courageCommand = new Command(
            "courage",
            "Get encouraged by an habit quote"
        );

        courageCommand.Arguments.Add(courageHabit);

        courageCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string habit = parseResult.GetValue(courageHabit)!;
                QuoteService.RandomQuote(habit);
            });
        });


        // Aliases
        // track <habit ...>
        var trackCommand = new Command("track", "Alias for habit add");

        trackCommand.Arguments.Add(habitAddHabits);
        trackCommand.Options.Add(habitAddHourGoal);
        trackCommand.Options.Add(habitAddPerDayGoal);

        trackCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string[] habits = parseResult.GetValue(habitAddHabits)!;
                double hourGoal = parseResult.GetValue(habitAddHourGoal);
                int perDayGoal = parseResult.GetValue(habitAddPerDayGoal);

                HabitService.AddHabit(habits, hourGoal, perDayGoal);
            });
        });

        // tracking
        var trackingCommand = new Command("tracking", "Alias for habit list");
        trackingCommand.SetAction(parseResult =>
        {
            SafeRun(() => HabitService.ListHabits());
        });

        // untrack <habit ...>
        var untrackCommand = new Command("untrack", "Alias for habit remove");
        untrackCommand.Arguments.Add(habitRemoveHabits);

        untrackCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string[] habits = parseResult.GetValue(habitRemoveHabits)!;
                HabitService.RemoveHabits(habits);
            });
        });

        // log <habit> <time> [-m --message TEXT] [-d --date DATE]
        var logCommand = new Command("log", "Alias for commit add");

        logCommand.Arguments.Add(commitAddHabit);
        logCommand.Arguments.Add(commitAddTime);
        logCommand.Options.Add(commitAddDate);
        logCommand.Options.Add(commitAddMessage);

        logCommand.SetAction(parseResult =>
        {
            SafeRun(() =>
            {
                string habit = parseResult.GetValue(commitAddHabit)!;
                string time = TimeParser.ParseTime(
                        parseResult.GetValue(commitAddTime)!
                ).ToString();

                string date = parseResult.GetValue(commitAddDate)!;
                if (date.Equals("today"))
                {
                    date = DateTime.Now.ToString();
                }

                string message = parseResult.GetValue(commitAddMessage)!;

                CommitService.AddCommit(
                    habit,
                    time,
                    date,
                    message
                );
            });
        });


        // initialize root command
        var rootCommand = new RootCommand(
            "Cli habit tracker, simple and pretty, written in C#."
        );

        // add subcommands
        // CRUD
        rootCommand.Subcommands.Add(habitCommand);
        rootCommand.Subcommands.Add(commitCommand);
        rootCommand.Subcommands.Add(quoteCommand);

        // Habit Tracking Utilities
        rootCommand.Subcommands.Add(courageCommand);
        rootCommand.Subcommands.Add(reportCommand);

        // Aliases
        rootCommand.Subcommands.Add(trackCommand);
        rootCommand.Subcommands.Add(trackingCommand);
        rootCommand.Subcommands.Add(untrackCommand);
        rootCommand.Subcommands.Add(logCommand);

        // execute action
        return rootCommand.Parse(args).Invoke();
    }
}

class HabitTrack
{
    public static TimeSpan GetTotalHabitEffort(int habitId)
    {
        var sum = new TimeSpan();

        List<TimeSpan> effortTimes = GetHabitEfforts(habitId);
        foreach(TimeSpan effortTime in effortTimes)
        {
            sum += effortTime;
        }
        return sum;
    }

    static List<TimeSpan> GetHabitEfforts(int habitId)
    {
        using var connection = Database.GetConnection();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT effort_time
            FROM commits
            WHERE habit_id = $habbit_id;
        """;
        command.Parameters.AddWithValue("$habbit_id", habitId.ToString());

        var habitTimes = new List<TimeSpan>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            habitTimes.Add(TimeSpan.Parse(reader.GetString(0)));
        }

        return habitTimes;
    }

    public static string GetProgressFill(
        TimeSpan time,
        double goalHours,
        int maxProgressSize
    )
    {
        char progressChar = '|';

        int progressCharCount = Convert.ToInt32(
             time.TotalHours * maxProgressSize / goalHours
        );
        # if DEBUG
            Console.WriteLine($"[DEBUG] time.TotalHours: {time.TotalHours}");
            Console.WriteLine($"[DEBUG] progressCharCount: {progressCharCount}");
        # endif

        return new string(progressChar, progressCharCount);
    }

    public static void PrintProgressBar(
        TimeSpan time,
        double hourGoal,
        int progressBarSize
    )
    {
        var colors = new List<ConsoleColor>{
            ConsoleColor.Green,
            ConsoleColor.Blue,
            ConsoleColor.Red,
            ConsoleColor.Yellow,
        };

        int maxProgressSize = progressBarSize - 4;

        var progress = GetProgressFill(time, hourGoal, maxProgressSize);

        string reducedProgress;
        if (progress.Length > maxProgressSize)
        {
            reducedProgress = progress[0..maxProgressSize];
        }
        else
        {
            reducedProgress = progress;
        }

        int excedent = progress.Length % maxProgressSize;

        int exceded = progress.Length / maxProgressSize;

        int i = exceded >= 0 ? exceded % colors.Count : 0;


        # if DEBUG
            Console.WriteLine($"[DEBUG] progress: {progress}");
            Console.WriteLine($"[DEBUG] reducedProgress: {reducedProgress}");
            Console.WriteLine($"[DEBUG] excedent: {excedent}");
            Console.WriteLine($"[DEBUG] exceded: {exceded}");
            Console.WriteLine($"[DEBUG] colorsIdx: {i}");
        # endif

        // PRINT BUILDING
        Console.Write("[ ");

        // first part
        Console.ForegroundColor = colors[i];
        Console.Write(reducedProgress[0..excedent]);

        // second and last part
        i = i == 0 ? colors.Count - 1 : i - 1;
        Console.ForegroundColor = colors[i];
        Console.Write(reducedProgress[excedent..]);

        int freeSpaceCount = maxProgressSize - reducedProgress.Length;
        var emptySpace = new string(' ', freeSpaceCount);

        Console.ResetColor();
        Console.Write($"{emptySpace} ]");
    }

    static Dictionary<DateTime, TimeSpan> GetCommitSample(int habitId)
    {
        var allCommits = CommitTable.GetOfHabit(habitId);
        allCommits.Sort((x, y) => DateTime.Compare(x.CommitTime, y.CommitTime));
        allCommits.Reverse();

        // debug
        # if DEBUG
            Console.WriteLine($"[DEBUG] allCommits.Count: {allCommits.Count}");
            for (int j = 0; j < allCommits.Count; j++)
            {
                Console.WriteLine($"[DEBUG] allCommits[{j}].Message: {allCommits[j].Message}");
                Console.WriteLine($"[DEBUG] allCommits[{j}].CommitTime: {allCommits[j].CommitTime}");
            }
        # endif

        if (allCommits.Count < 2)
        {
            throw new IndexOutOfRangeException("Not enough samples.");
        }

        var samples = new Dictionary<DateTime, TimeSpan>{
            {allCommits.First().CommitTime, allCommits.First().EffortTime}
        };

        # if DEBUG
            Console.WriteLine($"[DEBUG] Merging");
        # endif
        bool first = true;
        foreach (var commit in allCommits)
        {
            # if DEBUG
                Console.WriteLine($"[DEBUG] commit: {commit.EffortTime}");
                Console.WriteLine($"[DEBUG] commit: {commit.CommitTime}");
                Console.WriteLine($"[DEBUG] commit: {commit.Message}");
            # endif
            if (first)
            {
                first = false;
                continue;
            }
            else
            {
                first = false; // in bash, this is faster than always assigning
            }

            var k = samples.Last().Key;

            # if DEBUG
                Console.WriteLine($"[DEBUG] Key: {k}");
                Console.WriteLine($"[DEBUG] Value: {samples[k]}");
            # endif

            if (k.Date == commit.CommitTime.Date)
            {
                # if DEBUG
                    Console.WriteLine($"[DEBUG] added: {commit.EffortTime}");
                # endif
                samples[k] += commit.EffortTime;
            }
            else
            {
                samples.Add(commit.CommitTime, commit.EffortTime);
            }

            if (samples.Count == 2)
            {
                break;
            }
        }
        // debug
        # if DEBUG
            Console.WriteLine($"[DEBUG] samples.Count: {samples.Count}");
            var k1 = samples.First();
            var k2 = samples.Last();
            Console.WriteLine($"[DEBUG] 1 Key: {k1.Key}");
            Console.WriteLine($"[DEBUG] 1 Value: {k1.Value}");
            Console.WriteLine($"[DEBUG] 2 Key: {k2.Key}");
            Console.WriteLine($"[DEBUG] 2 Value: {k2.Value}");
        # endif

        if (samples.Count != 2)
        {
            throw new IndexOutOfRangeException("Not enough samples.");
        }

        return samples;
    }

    public static double GetRate(int habitId)
    {
        Dictionary<DateTime, TimeSpan> samples;
        samples = GetCommitSample(habitId);
        var day1 = samples.Last().Key;
        var day2 = samples.First().Key;
        var effort1 = samples.Last().Value;
        var effort2 = samples.First().Value;

        double deltaDays = (day2 - day1).TotalDays;
        double deltaHours = (effort2 - effort1).TotalHours;
        # if DEBUG
            Console.WriteLine($"[DEBUG] deltaDays = {deltaDays}");
            Console.WriteLine($"[DEBUG] deltaHours = {deltaHours}");
        # endif
        double rate = deltaHours/deltaDays;

        double goal = HabitTable.Get(habitId).HourGoal;
        // Judgement(rate, goal);

        return rate;
    }

    public static int GetStreak(int habitId)
    {
        int streakCount = 0;
        var allCommits = CommitTable.GetOfHabit(habitId);
        allCommits.Sort(
            (x, y) => DateTime.Compare(x.CommitTime, y.CommitTime)
        );
        allCommits.Reverse();

        var streakDay = DateTime.Today;
        foreach (var commit in allCommits)
        {
            # if DEBUG
                Console.WriteLine($"[DEBUG] are we going from present to the past?");
                Console.WriteLine($"[DEBUG] streakDay: {streakDay}");
                Console.WriteLine($"[DEBUG] commit.CommitTime: {commit.CommitTime}");
            # endif

            if(commit.CommitTime.Date == streakDay.AddDays(-streakCount).Date)
            {
                streakCount++;
                # if DEBUG
                    Console.WriteLine($"[DEBUG] streakCount: {streakCount}");
                # endif
            }
            else if (
                commit.CommitTime.Date != streakDay.AddDays(-streakCount + 1).Date
            )
            {
                break;
            }
        }
        // StreakJudgement(streakCount); // configurable? necessary?

        return streakCount;
    }

    // // DUMBEST ALGORITHMS KNOWN TO MAN BELOW vvvvvvvv
    // static void StreakJudgement(double value)
    // {
    //     // https://jamesclear.com/new-habit
    //     switch (value)
    //     {
    //         case 0:
    //             Console.Write("🔴");
    //             break;
    //         case <5:
    //             Console.Write("⭐");
    //             break;
    //         case <30:
    //             Console.Write("🌟");
    //             break;
    //         default:
    //             int rep = Convert.ToInt32(value) % 5;
    //             rep = rep == 0 ? 1 : rep;
    //             for (int i = 0; i < rep; i++)
    //             {
    //                 Console.Write("💫");
    //             }
    //             break;
    //     }
    // }
    //
    // static void Judgement(double value, double goal)
    // {
    //     // could be configurable
    //     // could be a switch case
    //     // lua scripts?
    //     if (value >= goal)
    //     {
    //         Console.WriteLine("✨");
    //     }
    //     else
    //     {
    //         Console.WriteLine("🫤");
    //     }
    // }
}

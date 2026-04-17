namespace Stakes.Core;


class ReportMetrics
{
    public static double GetTotalEffortInLastDays(string habitName, int days)
    {
        var today = DateTime.Today.Date;

        return (
            from commit
            in CommitTable.GetByHabit(habitName)
            where commit.Date.Date > today.AddDays(-days)
            where commit.Date.Date <= today
            select commit.Time
        ).Sum(t => t.TotalHours);
    }

    public static TimeSpan GetTotalEffort(string habitName)
    {
        var total = TimeSpan.Zero;

        IEnumerable<TimeSpan> times = HabitTable.GetEfforts(habitName);
        foreach(TimeSpan time in times)
        {
            total += time;
        }

        return total;
    }

    private static (DateTime, TimeSpan)[] GetCommitSamples(string habitName)
    {
        var habit = HabitTable.Get(habitName);
        var today = DateTime.Today.Date;
        var allCommits = (
            from commit
            in CommitTable.GetByHabit(habitName)
            orderby commit.Date descending
            where today.AddDays(-habit.PerDayGoal).Date <= commit.Date.Date
            where commit.Date.Date <= today.Date
            select commit
        ).ToList();

        if (allCommits.Count < 2)
        {
            throw new Exceptions.NotEnoughCommitsException();
        }

        var samples = new (DateTime Date, TimeSpan Time)[2];
        samples[0].Date = allCommits[0].Date.Date;
        samples[0].Time = allCommits[0].Time;

        int i = 0;
        foreach (var commit in allCommits.Skip(1))
        {
            if (samples[i].Date == commit.Date.Date)
            {
                samples[i].Time += commit.Time;
            }
            else
            {
                i++;
                if (i > 1)
                {
                    break;
                }
                samples[i].Date = commit.Date.Date;
                samples[i].Time = commit.Time;
            }
        }

        if (i < 1)
        {
            throw new Exceptions.NotEnoughCommitsException();
        }

        return samples;
    }

    public static double GetRate(string habitName)
    {
        (DateTime Date, TimeSpan Time)[] samples = GetCommitSamples(habitName);

        var day1 = samples[0].Date;
        var time1 = samples[0].Time;

        var day2 = samples[1].Date;
        var time2 = samples[1].Time;

        double rate = (time2 - time1).TotalHours/(day2 - day1).TotalDays;

        return rate;
    }

    public static int GetStreak(string habitName)
    {
        int streakCount = 0;
        var streakDay = DateTime.Today;

        var dates = (
            from commit
            in CommitTable.GetByHabit(habitName)
            orderby commit.Date descending
            where commit.Date.Date <= streakDay.Date
            select commit.Date
        ).Select(x => x.Date).Distinct().ToList();

        if (dates.Count == 0)
        {
            return 0;
        }

        if (!dates.Contains(streakDay))
        {
            streakDay = streakDay.AddDays(-1);
        }

        foreach (DateTime currDate in dates)
        {
            if(currDate.Date != streakDay.Date)
            {
                break;
            }
            streakDay = streakDay.AddDays(-1);
            streakCount++;
        }

        return streakCount;
    }
}

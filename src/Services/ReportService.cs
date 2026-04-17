namespace Stakes.Services;

using System.Text;
using Stakes.Core;
using Stakes.Models;


class ReportService
{
    private static Dictionary<string, bool> GetReportStyle(string style)
    {
        var styleParts = new Dictionary<string, bool>
        {
            { "name", true },
            { "progress", true },
            { "rate", true },
            { "streak", true }
        };

        if (string.IsNullOrWhiteSpace(style))
            return styleParts;

        var parts = style.Split(',');

        foreach (var raw in parts)
        {
            var part = raw.Trim().ToLower();

            if (part.Length < 2)
                throw new Exceptions.ReportStyleException();

            char op = part[0];
            string key = part[1..];

            if (op != '+' && op != '-')
                throw new Exceptions.ReportStyleException();

            if (!styleParts.ContainsKey(key))
                throw new Exceptions.ReportStyleException();

            styleParts[key] = op == '+';
        }

        return styleParts;
    }

    public static void Report(
        string[] habitNames,
        string style
    )
    {
        var styleParts = GetReportStyle(style);

        IEnumerable<Habit> habits;
        if (habitNames.Contains("all-habits"))
        {
            habits = HabitTable.GetAll();
        }
        else
        {
            habits = from name
                in habitNames
                select HabitTable.Get(name);
        }

        // Print report header
        Console.WriteLine("Progress:");

        var reportHeader = new StringBuilder();

        if (styleParts["name"])
        {
            reportHeader.Append("habit    ");
        }

        if (styleParts["progress"])
        {
            reportHeader.Append(StringFormatter.PadSides(
                "[progress-bar]",
                ProgressBar.Size
            ));
        }

        if (styleParts["rate"])
        {
            reportHeader.Append(" [rate] ");
        }

        if (styleParts["streak"])
        {
            reportHeader.Append(" (streak)");
        }

        Console.WriteLine(reportHeader);

        foreach (var habit in habits)
        {
            var habitReport = new StringBuilder();
            // Habit name
            if (styleParts["name"])
            {
                habitReport.Append(habit.Name.PadRight(9, ' '));
            }

            // Progress bar
            if (styleParts["progress"])
            {
                double realHours;
                if (habit.PerDayGoal == 0)
                {
                    realHours = ReportMetrics.GetTotalEffortInLastDays(
                        habit.Name,
                        habit.PerDayGoal
                    );
                }
                else
                {
                    realHours = ReportMetrics.GetTotalEffort(
                        habit.Name
                    ).TotalHours;
                }

                var progressBar = ProgressBar.GetProgressBar(
                    realHours,
                    habit.HourGoal
                );

                habitReport.Append(progressBar);
            }

            // Progress rate
            if (styleParts["rate"])
            {
                string rate;
                try
                {
                    var ft = "+0.##;-0.##;0.00";
                    rate = ReportMetrics.GetRate(habit.Name).ToString(ft);
                }
                catch (Exceptions.NotEnoughCommitsException)
                {
                    rate = "~";
                }
                habitReport.Append($" [{rate}]".PadRight(9, ' '));
            }

            // Streak
            if (styleParts["streak"])
            {
                var streak = ReportMetrics.GetStreak(habit.Name);
                var s = streak != 1 ? "s" : "";
                habitReport.Append($"({streak} day{s})");
            }

            Console.WriteLine(habitReport);
        }
    }
}

namespace Stakes.Services;

using Stakes.Core;


class HabitService
{
    public static void AddHabit(
        string[] habits,
        double hourGoal,
        int perDayGoal
    )
    {
        foreach (string habit in habits)
        {
            if (habit.Length > 8)
            {
                throw new Exceptions.HabitNameFormatException();
            }

            HabitTable.Add(habit, hourGoal, perDayGoal);

            Console.WriteLine($"Started to track habit: {habit}");
        }
    }

    public static void ListHabits()
    {
        Console.WriteLine("Tracking:");
        foreach (var habit in HabitTable.GetAll())
        {
            var time = ReportMetrics.GetTotalEffort(habit.Name);
            var name = habit.Name;
            var hourGoal = habit.HourGoal;
            var perDayGoal = habit.PerDayGoal;

            Console.WriteLine(
                $"- {name, -8}\t[{time}]\t({hourGoal}h/{perDayGoal}day)"
            );
        }
    }

    public static void UpdateHabit(
        string name,
        string newName,
        double? hourGoal,
        int? perDayGoal
    )
    {
        var habit = HabitTable.Get(name);

        if (hourGoal != null)
        {
            HabitTable.ChangeHourGoal(name, (double)hourGoal);
            if (hourGoal != habit.HourGoal)
            {
                Console.WriteLine(StringFormatter.FormatChanged(
                    habit.HourGoal.ToString(), ((double)hourGoal).ToString()
                ));
            }
        }

        if (perDayGoal != null)
        {
            HabitTable.ChangePerDayGoal(name, (int)perDayGoal);
            if (perDayGoal != habit.PerDayGoal)
            {
                Console.WriteLine(StringFormatter.FormatChanged(
                    habit.PerDayGoal.ToString(), ((int)perDayGoal).ToString()
                ));
            }
        }

        if (!newName.Equals(string.Empty))
        {
            HabitTable.Rename(name, newName);
            if (!newName.Equals(habit.Name))
            {
                Console.WriteLine(
                    StringFormatter.FormatChanged(habit.Name, newName)
                );
            }
        }
    }

    public static void RemoveHabits(string[] habits)
    {
        foreach (string habit in habits)
        {
            HabitTable.Get(habit); // error catching
            HabitTable.Remove(habit);
            Console.WriteLine($"Untracking habit: {habit}");
        }
    }
}

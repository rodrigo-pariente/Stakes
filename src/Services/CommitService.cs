namespace Stakes.Services;

using Stakes.Core;
using Stakes.Models;


class CommitService
{
    public static void AddCommit(
        string habitName,
        string time,
        string date,
        string message
    )
    {
        CommitTable.Add(habitName, time, date, message);

        Console.WriteLine("Commited:");
        Console.WriteLine($"{date} I dedicated {time} to {habitName}");
        if (!message.Equals(string.Empty))
        {
            Console.WriteLine($"\"{message}\"");
        }
    }

    public static void ListCommits()
    {
        Console.WriteLine("Commit History:");
        foreach (Commit commit in CommitTable.GetAll())
        {
            var id = commit.Id;
            var habitName = commit.HabitName;
            var date = commit.Date;
            var time = commit.Time;

            Console.Write(
                $"{id}. {habitName, -9}" + $"[{date}] ".PadRight(25, ' ') +
                $"({time})"
            );

            if (!commit.Message.Equals(string.Empty))
            {
                Console.Write($" \"{commit.Message}\"");
            }

            Console.WriteLine();
        }
    }

    public static void UpdateCommit(
        int commitId,
        string habitName,
        string time,
        string date,
        string message
    )
    {
        var commit = CommitTable.Get(commitId);

        if (!habitName.Equals(string.Empty))
        {
            CommitTable.UpdateHabit(commitId, habitName);
            if (!habitName.Equals(commit.HabitName))
            {
                Console.WriteLine(
                    StringFormatter.FormatChanged(commit.HabitName, habitName)
                );
            }
        }

        if (!time.Equals(string.Empty))
        {
            CommitTable.UpdateTime(commitId, time);
            if (!time.Equals(commit.Time.ToString()))
            {
                Console.WriteLine(StringFormatter.FormatChanged(
                    commit.Time.ToString(), time
                ));
            }
        }

        if (!date.Equals(string.Empty))
        {
            CommitTable.UpdateDate(commitId, date);
            if (!date.Equals(commit.Date.ToString()))
            {
                Console.WriteLine(StringFormatter.FormatChanged(
                    commit.Date.ToString(), date
                ));
            }
        }

        if (!message.Equals(string.Empty))
        {
            CommitTable.UpdateMessage(commitId, message);
            if (!message.Equals(commit.Message))
            {
                Console.WriteLine(
                    StringFormatter.FormatChanged(commit.Message, message)
                );
            }
        }
    }

    public static void RemoveCommits(int[] commitIds)
    {
        foreach (int commitId in commitIds)
        {
            CommitTable.Get(commitId); // error catching
            CommitTable.Remove(commitId);
            Console.WriteLine($"Uncommiting commit: {commitId}");
        }
    }
}

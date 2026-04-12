class CommitTable
{
    public record Commit(
        int Id,
        string HabitName,
        TimeSpan EffortTime,
        DateTime CommitDate,
        string Message
    );

    public static void Add(
        string habitName,
        string effortTime,
        string commitDate,
        string message = ""
    )
    {

        string command = """
            INSERT INTO commits (
                habit_id,
                effort_time,
                commit_time,
                message
            )
            VALUES (
                (SELECT id FROM habits WHERE name = $habitName),
                $effortTime,
                $commitDate,
                $message
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$habitName", habitName},
            {"$effortTime", effortTime},
            {"$commitDate", commitDate},
            {"$message", message}
        };

        # if DEBUG
            Console.WriteLine($"[DEBUG] habitName: {habitName}");
            Console.WriteLine($"[DEBUG] effortTime: {effortTime}");
            Console.WriteLine($"[DEBUG] commitDate: {commitDate}");
            Console.WriteLine($"[DEBUG] message: {message}");
        # endif
        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new IndexOutOfRangeException("No habit of given name!");
            // this is so unclassy omg
        }
    }

    public static Commit Get(int commitId)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT commits.id, habits.name, effort_time, commit_time, message
            FROM commits
            INNER JOIN habits
            ON habits.id = commits.habit_id
            WHERE commits.id = $commit_id
        """;
        command.Parameters.AddWithValue("$commit_id", commitId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var commit = new Commit(
                reader.GetInt32(0),
                reader.GetString(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            return commit;
        }
        // will it ever happen?
        throw new IndexOutOfRangeException("No quote of commit id!");
    }

    public static List<Commit> GetAll()
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT commits.id, habits.name, effort_time, commit_time, message
            FROM commits
            INNER JOIN habits
            ON habits.id = commits.habit_id
        """;

        var commits = new List<Commit>();
        Commit commit;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            commit = new Commit(
                reader.GetInt32(0),
                reader.GetString(1),
                TimeSpan.Parse(reader.GetString(2)),
                DateTime.Parse(reader.GetString(3)),
                reader.GetString(4)
            );
            commits.Add(commit);
        }

        return commits;
    }

    public static List<Commit> GetOfHabit(string habitName)
    {
        using var connection = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM commits
            WHERE habit_id = (SELECT id FROM habits WHERE name = $habit_name)
        """;
        command.Parameters.AddWithValue("habit_name", habitName);

        var commits = new List<Commit>();
        Commit commit;

        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                commit = new Commit(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    TimeSpan.Parse(reader.GetString(2)),
                    DateTime.Parse(reader.GetString(3)),
                    reader.GetString(4)
                );
                commits.Add(commit);
            }
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new IndexOutOfRangeException("No habit of given name!");
        }

        return commits;
    }

    public static void Update(
        int commitId,
        string habitName = "",
        string effortTime = "",
        string commitDate = "",
        string message = ""
    )
    {
        using var connection = Database.GetConnection();

        Commit commit = Get(commitId);
        # if DEBUG
            Console.WriteLine($"[DEBUG] commit.HabitName: {commit.HabitName}");
            Console.WriteLine($"[DEBUG] commit.EffortTime: {commit.EffortTime}");
            Console.WriteLine($"[DEBUG] commit.CommitDate: {commit.CommitDate}");
            Console.WriteLine($"[DEBUG] commit.Message: {commit.Message}");

            Console.WriteLine($"[DEBUG] habitName: {habitName}");
            Console.WriteLine($"[DEBUG] effortTime: {effortTime}");
            Console.WriteLine($"[DEBUG] commitDate: {commitDate}");
            Console.WriteLine($"[DEBUG] message: {message}");
        # endif

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT commits.id, habits.name, effort_time, commit_time, message
            FROM commits
            INNER JOIN habits
            ON habits.id = commits.habit_id
            WHERE commits.id = $commit_id
        """;

        command.Parameters.AddWithValue(
            "$commit_id", commitId.ToString()
        );
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            if (habitName.Equals(string.Empty))
            {
                habitName = reader.GetString(1);
            }
            if (effortTime.Equals(string.Empty))
            {
                effortTime = reader.GetString(2);
            }
            if (commitDate.Equals(string.Empty))
            {
                commitDate = reader.GetString(3);
            }
            if (message.Equals(string.Empty))
            {
                message = reader.GetString(4);
            }
        }

        string cmd = """
            UPDATE commits
            SET habit_id = (SELECT id FROM habits WHERE name = $habit_name),
                effort_time = $effort_time,
                commit_time = $commit_time,
                message = $message
            WHERE id = $commit_id
        """;

        var parameters = new Dictionary<string, string>{
            {"$habit_name", habitName},
            {"$effort_time", effortTime},
            {"$commit_time", commitDate},
            {"$message", message},
            {"$commit_id", commitId.ToString()}
        };


        try
        {
            Database.ExecuteCommand(cmd, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new IndexOutOfRangeException("ERROR: no commit of given id");
        }
    }

    public static void Remove(int commitId)
    {
        string command = """
            DELETE FROM commits
            WHERE id = $commit_id
        """;
        var parameters = new Dictionary<string, string>{
            {"$commit_id", commitId.ToString()}
        };

        try
        {
            Database.ExecuteCommand(command, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new IndexOutOfRangeException("ERROR: no commit of given id");
        }
    }
}

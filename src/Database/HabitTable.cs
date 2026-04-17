using Stakes.Core;
using Stakes.Models;


class HabitTable
{
    public static void Add(string name, double hourGoal, int perDayGoal)
    {
        string addHabit = """
            INSERT INTO habits (name, hour_goal, per_day_goal)
            VALUES (
                $name,
                $hour_goal,
                $per_day_goal
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$name", name},
            {"$hour_goal", hourGoal.ToString()},
            {"$per_day_goal", perDayGoal.ToString()}
        };

        try
        {
            Database.ExecuteCommand(addHabit, parameters);
        }
        catch (Microsoft.Data.Sqlite.SqliteException)
        {
            throw new Exceptions.HabitNameNotUniqueException();
        }
    }

    public static void Rename(string name, string newName)
    {
        string renameHabit = """
            UPDATE habits
            SET name = $new_name
            WHERE name = $name;
        """;

        var parameters = new Dictionary<string, string>{
            {"$new_name", newName},
            {"$name", name}
        };

        Database.ExecuteCommand(renameHabit, parameters);
    }


    public static void ChangeHourGoal(string name, double hourGoal)
    {
        string changeHourGoal = """
            UPDATE habits
            SET hour_goal = $hour_goal
            WHERE name = $name;
        """;

        var parameters = new Dictionary<string, string>{
            {"$hour_goal", hourGoal.ToString()},
            {"$name", name}
        };

        Database.ExecuteCommand(changeHourGoal, parameters);
    }

    public static void ChangePerDayGoal(string name, int perDayGoal)
    {
        string changePerDayGoal = """
            UPDATE habits
            SET per_day_goal = $per_day_goal
            WHERE name = $name;
        """;

        var parameters = new Dictionary<string, string>{
            {"$per_day_goal", perDayGoal.ToString()},
            {"$name", name}
        };

        Database.ExecuteCommand(changePerDayGoal, parameters);
    }

    public static List<Habit> GetAll()
    {
        using var connection  = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM habits;
        """;

        var habits = new List<Habit>();
        Habit habit;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            habit = new Habit(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetInt32(3)
            );
            habits.Add(habit);
        }
        return habits;
    }

    public static Habit Get(string name)
    {
        using var connection  = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM habits
            WHERE id = (
                SELECT id
                FROM habits
                WHERE name = $habit_name
            );
        """;

        command.Parameters.AddWithValue("$habit_name", name);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var habit = new Habit(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetInt32(3)
            );
            return habit;
        }

        throw new Exceptions.HabitNotFoundException(name);
    }

    public static IEnumerable<TimeSpan> GetEfforts(string name)
    {
        using var connection = Database.GetConnection();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT time
            FROM commits
            WHERE habit_id = (
                SELECT id
                FROM habits
                WHERE name = $name
            );
        """;
        command.Parameters.AddWithValue("$name", name);

        var habitTimes = new List<TimeSpan>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            habitTimes.Add(TimeSpan.Parse(reader.GetString(0)));
        }

        return habitTimes;
    }

    public static void Remove(string name)
    {
        string removeHabit = """
            DELETE FROM habits
            WHERE name = $name;
        """;

        var parameters = new Dictionary<string, string>{
            {"$name", name}
        };

        Database.ExecuteCommand(removeHabit, parameters);
    }
}

using Microsoft.Data.Sqlite;


class HabitTable
{
    public record Habit(int Id, string Name, double HourGoal);

    public static void Add(string name, double hourGoal)
    {
        string addHabit = """
            INSERT INTO habits (name, hour_goal)
            VALUES (
                $name,
                $hour_goal
            );
        """;

        var parameters = new Dictionary<string, string>{
            {"$name", name},
            {"$hour_goal", hourGoal.ToString()}
        };

        Database.ExecuteCommand(addHabit, parameters);
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

    // public static int GetId(string name)
    // {
    //     using var connection = Database.GetConnection();
    //     var command = connection.CreateCommand();
    //     command.CommandText = """
    //         SELECT id
    //         FROM habits
    //         WHERE name = $name;
    //     """;
    //
    //     command.Parameters.AddWithValue("$name", name);
    //
    //     // is it useful?
    //     var noHabit = new IndexOutOfRangeException("No habit of given name!");
    //     try
    //     {
    //         var reader = command.ExecuteReader();
    //
    //         while (reader.Read())
    //         {
    //             return reader.GetInt32(0);
    //         }
    //     }
    //     catch (SqliteException)
    //     {
    //         throw noHabit;
    //     }
    //     throw noHabit;
    // }

    public static Habit Get(string habitName)
    {
        using var connection  = Database.GetConnection();
        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT *
            FROM habits
            WHERE id = (SELECT id FROM habits WHERE name = $habit_name)
        """;

        command.Parameters.AddWithValue("$habit_name", habitName);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var habit = new Habit(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2)
            );
            return habit;
        }
        throw new IndexOutOfRangeException("No habit of given name!");
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
                reader.GetDouble(2)
            );
            habits.Add(habit);
        }
        return habits;
    }
}

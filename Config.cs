using System.Text.Json;


class Config
{
    public record Configuration(
        string DatabasePath = "/opt/Stakes/stakes.db"
        // int HourGoal = 1,
        // string ReportStyle = "+progress,+rate,+streak",
        // string ProgressColors = "red,yellow,green,blue"
    );

    public static string configurationPath = "/opt/Stakes/config.json";

    public static Configuration Load()
    {
        if (!File.Exists(configurationPath))
        {
            return new Configuration();
        }

        var json = File.ReadAllText(configurationPath);
        var config = JsonSerializer.Deserialize<Configuration>(json)
            ?? new Configuration();
        # if DEBUG
            Console.WriteLine($"[DEBUG] config: {config}");
        # endif
        return config;
    }

    public static void Create()
    {
        string json = JsonSerializer.Serialize(new Configuration());
        File.WriteAllText(configurationPath, json);
    }
}

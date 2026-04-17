namespace Stakes.Core;

using System.Text.Json;
using Stakes.Models;


class Config
{
    public static string applicationPath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData
        ),
        "Stakes/"
    );

    public static string configurationPath = Path.Combine(
        applicationPath, "config.json"
    );

    public static void Create()
    {
        string json = JsonSerializer.Serialize(new Configuration());
        try
        {
            File.WriteAllText(configurationPath, json);
        }
        catch(UnauthorizedAccessException)
        {
            throw new Exceptions.UnauthorizedFileCreationException(
                "configuration file"
            );
        }
    }

    public static Configuration Load()
    {
        if (!File.Exists(configurationPath))
        {
            return new Configuration(
                Path.Combine(applicationPath, "stakes.db")
            );
        }

        try
        {
            var json = File.ReadAllText(configurationPath);
            var config = JsonSerializer.Deserialize<Configuration>(json)
                ?? new Configuration();
            return config;
        }
        catch(FileNotFoundException)
        {
            return new Configuration(
                Path.Combine(applicationPath, "stakes.db")
            );
        }
    }
}

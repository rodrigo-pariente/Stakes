namespace Stakes.Core;


class InitializeApplication
{
    public static void InitializeApplicationFolder()
    {
        // Create stakes folder if does not exist
        if (!Directory.Exists(Config.applicationPath))
        {
            try
            {
                Directory.CreateDirectory(Config.applicationPath);
            }
            catch(UnauthorizedAccessException)
            {
                throw new Exceptions.UnauthorizedDirectoryCreationException(
                    "application directory"
                );
            }
        }

        // Create configuration if does not exist
        if (!File.Exists(Config.configurationPath))
        {
            Config.Create();
        }

        // Guarantee the database existence and conformity
        Database.Initialize();
    }
}

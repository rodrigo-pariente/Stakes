namespace Stakes.Core;


class Exceptions
{
    // Base Exception
    public class StakesException : Exception
    {
        public StakesException() {}

        public StakesException(string message)
            : base(message) {}
    }

    // Program Exceptions
    public class UnauthorizedDirectoryCreationException : StakesException
    {
        public UnauthorizedDirectoryCreationException()
            : base(
                "Stakes don't have permission to create directory.\n" +
                "Try running again with higher privileges." 
            ) {}

        public UnauthorizedDirectoryCreationException(string directory)
            : base(
                "Stakes don't have permission to create directory " +
                $"{directory}.\nTry running again with higher privileges." 
            ) {}
    }

    public class UnauthorizedFileCreationException : StakesException
    {
        public UnauthorizedFileCreationException()
            : base(
                "Stakes don't have permission to create file.\n" +
                "Try running again with higher privileges." 
            ) {}

        public UnauthorizedFileCreationException(string file)
            : base(
                "Stakes don't have permission to create file " +
                $"{file}.\nTry running again with higher privileges." 
            ) {}
    }

    // Database Exceptions
    public class HabitNotFoundException : StakesException
    {
        public HabitNotFoundException()
            : base("Habit not found.") {}

        public HabitNotFoundException(string name)
            : base($"Habit \"{name}\" not found.") {}
    }

    public class HabitNameFormatException : StakesException
    {
        public HabitNameFormatException()
            : base("Habit name must have at maximum 8 chars") {}
    }

    public class HabitNameNotUniqueException : StakesException
    {
        public HabitNameNotUniqueException()
            : base("Habit name must be unique.") {}
    }

    public class CommitNotFoundException : StakesException
    {
        public CommitNotFoundException()
            : base("Commit not found.") {}

        public CommitNotFoundException(int id)
            : base($"Commit of id \"{id}\" not found.") {}
    }

    public class QuoteNotFoundException : StakesException
    {
        public QuoteNotFoundException()
            : base("Quote not found.") {}

        public QuoteNotFoundException(int id)
            : base($"Quote of id \"{id}\" not found.") {}
    }

    // Service Exceptions
    public class ReportStyleException : StakesException
    {
        public ReportStyleException()
            : base(
                "Bad report style format.\n" +
                "Example: --style '+name,+progress,-rate,-streak'\n" +
                "Where '+' ensures showing and '-' hides the element."
            ) {}
    }

    public class ColorStringException : StakesException
    {
        public ColorStringException()
            : base(
                "Bad color string format.\n" +
                "Example: --colors '#FF5555,#F1FA8C,#50FA7B,#BD93F9'"
            ) {}
    }

    public class ProgressBarSizeException : StakesException
    {
        public ProgressBarSizeException()
            : base("Progress bar size must be 1 char or higher.") {}
    }


    // ReportMetric Exceptions
    public class NotEnoughCommitsException: StakesException
    {
        public NotEnoughCommitsException()
            : base("Not enough commit samples.") {}
    }

    // Parse Exceptions
    public class TimeFormatException : StakesException
    {
        public TimeFormatException()
            : base("Bad time format.") {}
    }

    public class DateTimeFormatException : StakesException
    {
        public DateTimeFormatException()
            : base("Bad datetime format.") {}
    }
}

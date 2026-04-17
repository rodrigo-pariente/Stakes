namespace Stakes.Models;


public record Commit(
    int Id,
    string HabitName,
    TimeSpan Time,
    DateTime Date,
    string Message
);

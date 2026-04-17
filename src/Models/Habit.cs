namespace Stakes.Models;


public record Habit(
    int Id,
    string Name,
    double HourGoal,
    int PerDayGoal
);

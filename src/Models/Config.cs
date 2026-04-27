namespace Stakes.Models;


public record Configuration(
    double HourGoal = 1D,
    int PerDayGoal = 1,
    string ReportStyle = "+name,+progress,+rate,+streak",
    char ProgressChar = '|',
    char AltProgressChar = '|',
    char EmptyProgressChar = ' ',
    string ProgressColors = "#FF5555,#F1FA8C,#50FA7B,#BD93F9"
);

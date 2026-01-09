namespace StudentProgressTracker.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public int InactivityWarningMinutes { get; set; } = 2;
    public int InactivityAlertMinutes { get; set; } = 5;
    public string BaseUrl { get; set; } = "https://localhost:7001";
}

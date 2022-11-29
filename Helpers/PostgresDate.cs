namespace BugTrackerMVC.Helpers
{
    public class PostgresDate
    {
        public static DateTime Format(DateTime datetime)
        {
            return DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
        }
    }
}

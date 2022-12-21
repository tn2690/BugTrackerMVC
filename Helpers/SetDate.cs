namespace BugTrackerMVC.Helpers
{
    public static class SetDate
    {
        public static DateTime Format(DateTime? datetime)
        {
            return DateTime.SpecifyKind(datetime!.Value, DateTimeKind.Utc);
        }
    }
}

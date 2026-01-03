namespace dasarapaymenttracker.Services;

public static class MonthLockService
{
    public static DateOnly? LockedMonth(DateTime now)
    {
        if (now.Day <= 15) return null;
        return new DateOnly(now.Year, now.Month, 1).AddMonths(-1);
    }

    public static bool IsLocked(DateTime now, DateOnly month)
        => LockedMonth(now) == month;
}

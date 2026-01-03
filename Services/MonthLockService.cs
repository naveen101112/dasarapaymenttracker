namespace dasarapaymenttracker.Services;

public static class MonthLockService
{
    public static (DateOnly? Prev, DateOnly? Curr) LockedMonths(DateTime nowLocal)
    {
        if (nowLocal.Day <= 15) return (null, null);

        var curr = new DateOnly(nowLocal.Year, nowLocal.Month, 1);
        var prev = curr.AddMonths(-1);
        return (prev, curr);
    }

    public static bool IsLocked(DateTime nowLocal, DateOnly monthStart)
    {
        var (prev, curr) = LockedMonths(nowLocal);
        return (prev.HasValue && prev.Value == monthStart) ||
               (curr.HasValue && curr.Value == monthStart);
    }
}

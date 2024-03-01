namespace Anthill.Schedule;

internal sealed class ScheduleItem
{
    public ScheduleItem(string name, TimeSpan period, Action action)
    {
        this.Name = name;
        this.Period = period;
        this.Action = action;
    }

    public string Name { get; }
    public TimeSpan Period { get;  }
    internal DateTimeOffset NextRun { get; set; }
    public Action Action { get; }
    public bool Running { get; set; } = false;
}

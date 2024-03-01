namespace Anthill.Schedule;

public class ScheduleException : Exception
{
    public ScheduleException(string name, Exception innerException) : base($"Schedule {name}",  innerException)
    {
    }
}

using Anthill.Schedule;

namespace Anthill.Hub;

public static class TrailConfigurationExtensions
{
    public static Trail AddQueue(this Trail trail, int maxThreads = 8)
    {
        var executor = new Executor.TaskQueue(trail);
        executor.MaxRunners = maxThreads;
        trail.RegisterPlugin(executor);
        return trail;
    }

    public static Trail AddScheduler(this Trail trail)
    {
        var scheduleRunner = new ScheduleRunner(trail);
        trail.RegisterPlugin(scheduleRunner);
        return trail;
    }
}

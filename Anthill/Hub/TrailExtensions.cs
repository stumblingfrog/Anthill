namespace Anthill.Hub;

using Anthill.Core;
using Anthill.Executor;
using Anthill.Schedule;

using System.Diagnostics;

public static class TrailExtensions
{
    public static void Schedule(this Trail trail, string name, Action action, TimeSpan period)
    {
        Debug.Assert(trail != null);
        Debug.Assert(trail.IsRegistered<TaskQueue>(), "To use Shedule method, you need to add queue plugin (trail.AddQueue())");
        Debug.Assert(trail.IsRegistered<ScheduleRunner>(), "To use Shedule method, you need to add shedule plugin (trail.AddSchedule())");

        var scheduleItem = new ScheduleItem(name, period, action);
        trail.Publish(scheduleItem);
    }

    public static void Queue(this Trail trail, Action<Trail> action)
    {
        Debug.Assert(trail != null);
        Debug.Assert(trail.IsRegistered<TaskQueue>(), "To use Queue method, you need to add queue plugin (trail.AddQueue())");

        trail.Queue(() => action(trail));
    }

    public static void Queue(this Trail trail, Action action)
    {
        Debug.Assert(trail != null);
        Debug.Assert(trail.IsRegistered<TaskQueue>(), "To use Queue method, you need to add queue plugin (trail.AddQueue())");

        trail.Publish(new QueueTask(action));
    }

    public static void Start(this Trail trail)
    {
        trail.Publish(RunState.Run);
    }

    public static void Pause(this Trail trail)
    {
        trail.Publish(RunState.Pause);
    }

    public static void Stop(this Trail trail)
    {
        trail.Publish(RunState.Stop);
    }
}

namespace Anthill.Schedule;

using Anthill.Core;
using Anthill.Executor;
using Anthill.Hub;

internal sealed class ScheduleRunner
{
    private readonly AutoResetEvent waitEvent = new(true);
    private readonly List<ScheduleItem> schedules = [];
    private readonly ThreadLock threadLock = new ();
    private readonly Trail trail;
    private readonly Task scheduleTask;
    
    private RunState state = RunState.Run;

    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    public ScheduleRunner(Trail trail)
    {
        this.trail = trail;
        this.state = RunState.Run;
        this.scheduleTask = Task.Run(this.ScheduleHandler);
        this.trail.Subscribe<RunState>(StateChanged);
        this.trail.Subscribe<ScheduleItem>(Schedule);
    }

    private void Schedule(ScheduleItem item)
    {
        using (this.threadLock.Lock())
        {
            item.NextRun = this.TimeProvider.GetUtcNow();
            schedules.Add(item);
            this.waitEvent.Set();
        }
    }

    private void StateChanged(RunState state)
    {
        this.state = state;
        switch (state)
        {
            case RunState.Stop:
                this.StopInternal();
                break;
        }
    }

    private void StopInternal()
    {
        this.state = RunState.Stop;
        if (this.scheduleTask == null)
        {
            return;
        }
        this.waitEvent.Set();
        this.scheduleTask.Wait();
    }

    private void ScheduleHandler()
    {
        while (true)
        {
            if (this.state == RunState.Stop)
            {
                return;
            }

            if (this.state == RunState.Pause)
            {
                this.waitEvent.WaitOne();
            }

            var now = this.TimeProvider.GetUtcNow();
            var nextRun = DateTimeOffset.MaxValue;
            using (this.threadLock.Lock())
            {
                for (var i = 0; i < this.schedules.Count; i++)
                {
                    var schedule = this.schedules[i];
                    if (!schedule.Running && schedule.NextRun<=now) 
                    {
                        schedule.Running = true;
                        this.trail.Queue(this.ScheduleItemRun(schedule, now));
                    }
                    
                    if(nextRun > schedule.NextRun)
                    {
                        nextRun = schedule.NextRun;
                    }
                }
            }
            if (nextRun != DateTimeOffset.MaxValue && nextRun > now)
            {
                this.waitEvent.WaitOne(nextRun - now);
            }
            else
            {
                this.waitEvent.WaitOne();
            }
        }
    }

    private Action ScheduleItemRun(ScheduleItem item, DateTimeOffset now)
    {
        return () =>
        {
            try
            {
                item.Action.Invoke();
            }
            catch(Exception e)
            {
                throw new ScheduleException(item.Name, e);
            }
            finally
            {
                item.Running = false;
                item.NextRun = now + item.Period;
                this.waitEvent.Set();
            }
        };
    }
}

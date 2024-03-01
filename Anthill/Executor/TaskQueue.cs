namespace Anthill.Executor;

using Anthill.Core;
using Anthill.Hub;

internal sealed class TaskQueue
{
    private readonly List<TaskRunner> runners = [];
    private readonly Queue<Action> queue = [];
    private readonly ThreadLock threadLock = new();
    private readonly Trail trail;

    public TaskQueue(Trail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        this.trail = trail;

        this.trail.Subscribe<RunState>(ChangeState);
        this.trail.Subscribe<TaskRunner>(ExecutionFinished);
        this.trail.Subscribe<QueueTask>(item => this.Queue(item.Action));
    }

    public int MaxRunners { get; set; } = 4;
        
    private void Queue(Action action)
    {
        using var _ = threadLock.Lock();

        var runner = FindRunner();
        if (runner != null)
        {
            runner.Run(action);
        }
        else
        {
            queue.Enqueue(action);
        }
    }

    private void ChangeState(RunState state)
    {
        switch (state)
        {
            case RunState.Stop:
                this.StopInternal();
                break;
        }
    }

    private TaskRunner? FindRunner()
    {
        for (var i = 0; i < runners.Count; i++)
        {
            var runner = runners[i];
            if (runner.Idle)
            {
                return runner;
            }
        }

        if (runners.Count < MaxRunners)
        {
            var runner = new TaskRunner(this.trail);
            runners.Add(runner);
            return runner;
        }

        return null;
    }

    private void ExecutionFinished(TaskRunner runner)
    {
        using (threadLock.Lock())
        {
            runner.Task = null;
            if (queue.TryDequeue(out var action))
            {
                runner.Run(action);
            }
        }
    }

    private void StopInternal()
    {
        while (true)
        {
            var tasks = GetTasks();
            Task.WaitAll(tasks);
            if (queue.Count == 0)
            {
                tasks = GetTasks();
                Task.WaitAll(tasks);

                return;
            }
        }
    }

    private Task[] GetTasks()
    {
        using (threadLock.Lock())
        {
            return runners
                .Select(x => x.Task)
                .Where(x => x != null)
                .Select(x => x!)
                .ToArray();
        }
    }
}
namespace Anthill.Executor;

using Anthill.Hub;

internal sealed class TaskRunner
{
    private readonly Trail trail;

    internal Task? Task { get; set; } = null;

    public bool Idle { get => Task == null; }

    public TaskRunner(Trail trail)
    {
        ArgumentNullException.ThrowIfNull(trail);
        this.trail = trail;
    }

    internal void Run(Action action)
    {
        Task = Task.Run(() => RunInternal(action));
    }

    private void RunInternal(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception exception)
        {
            trail.Publish(exception);
        }
        this.trail.Publish(this);
    }
}

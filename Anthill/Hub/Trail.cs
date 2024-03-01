namespace Anthill.Hub;

public sealed class Trail
{
    private readonly ReaderWriterLockSlim readerWriterLock = new();
    private readonly List<IHandler> handlers = [];
    private readonly List<object> plugins = [];

    public void Subscribe<T>(Action<T> action, bool withAncestors = false)
    {
        this.GetHandler<T>(withAncestors).AddSubsciber(action);
    }

    public void Publish<T>(T message)
    {
        ArgumentNullException.ThrowIfNull(message);

        this.FindHandler<T>()?.Handle(message);
    }

    internal void RegisterPlugin<T>(T plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        this.readerWriterLock.EnterUpgradeableReadLock();
        try
        {
            if (this.IsRegistered<T>())
            {
                throw new ArgumentException($"Plugin of type {typeof(T)} already registered");
            }

            this.readerWriterLock.EnterWriteLock();
            try
            {
                this.plugins.Add(plugin);
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }
        finally
        {
            this.readerWriterLock.ExitUpgradeableReadLock();
        }
    }

    internal bool IsRegistered<T>()
    {
        this.readerWriterLock.EnterReadLock();
        try
        {
            var requiredType = typeof(T);
            return plugins.Any(x => x.GetType() == requiredType);
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
            
    }

    private Handler<T> GetHandler<T>(bool withAncestors)
    {
        this.readerWriterLock.EnterUpgradeableReadLock();
        try
        {
            for (var i = 0; i < this.handlers.Count(); i++)
            {
                var current = handlers[i];
                if (current.IsMatch<T>())
                {
                    if (typeof(T) != current.SubscriberType)
                    {
                        var newHandler = Handler<T>.Create(withAncestors);
                        this.readerWriterLock.EnterWriteLock();
                        try
                        {
                            this.handlers.Insert(i, newHandler);
                            return newHandler;
                        }
                        finally
                        {
                            this.readerWriterLock.ExitWriteLock();
                        }
                    }
                    return (Handler<T>)current;
                }
            }

            this.readerWriterLock.EnterWriteLock();
            try
            {
                var handler = Handler<T>.Create(withAncestors);
                this.handlers.Add(handler);
                return handler;
            }
            finally
            {
                this.readerWriterLock.ExitWriteLock();
            }
        }
        finally
        {
            this.readerWriterLock.ExitUpgradeableReadLock();
        }
    }

    private IHandler? FindHandler<T>()
    {
        this.readerWriterLock.EnterReadLock();
        try
        {
            for (var i = 0; i < this.handlers.Count(); i++)
            {
                var current = handlers[i];
                if (current.IsMatch<T>())
                {
                    return current;
                }
            }
            return null;
        }
        finally
        {
            this.readerWriterLock.ExitReadLock();
        }
    }
}

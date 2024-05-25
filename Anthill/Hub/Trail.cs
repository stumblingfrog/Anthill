using System.Buffers;
using System.Collections.Generic;

namespace Anthill.Hub;

public sealed class Trail
{
    private readonly ReaderWriterLockSlim readerWriterLock = new();
    private readonly List<IHandler> handlers = [];
    private readonly List<object> plugins = [];

    public void Subscribe<T>(Action<T> action, bool withAncestors = false)
    {
        this.GetOrCreateHandler<T>(withAncestors).AddSubsciber(action);
    }

    public void Publish<T>(T message)
    {
        ArgumentNullException.ThrowIfNull(message);

        this.HandleMessage<T>(message);
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

    private Handler<T> GetOrCreateHandler<T>(bool withAncestors)
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

    private void HandleMessage<T>(T message)
    {
        var handlersCount = this.handlers.Count();
        var activeHandlers = ArrayPool<IHandler>.Shared.Rent(handlersCount);
        try
        {
            var count = 0;
            this.readerWriterLock.EnterReadLock();
            try
            {
                for (var i = 0; i < handlersCount; i++)
                {
                    var current = handlers[i];
                    if (current.IsMatch<T>())
                    {
                        activeHandlers[count] = current;
                        count++;
                    }
                }
            }
            finally
            {
                this.readerWriterLock.ExitReadLock();
            }

            for (var i = 0; i < count; i++)
            {
                activeHandlers[i].Handle(message);
            }
        }
        finally
        {
            ArrayPool<IHandler>.Shared.Return(activeHandlers);
        }
    }
}

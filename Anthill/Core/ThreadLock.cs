namespace Anthill.Core;

internal sealed class ThreadLock
{
    private readonly Mutex mutex = new (false);

    public LockItem Lock()
    {
        return new LockItem(this.mutex);
    }

    internal readonly ref struct LockItem 
    {
        private readonly Mutex mutex;

        public LockItem(Mutex mutex)
        {
            this.mutex = mutex;
            this.mutex.WaitOne();
        }
        
        public void Dispose()
        {
            this.mutex.ReleaseMutex();
        }
    }
}

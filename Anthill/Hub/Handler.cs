namespace Anthill.Hub;

using System.Runtime.InteropServices;

internal class Handler<T> : IHandler
{
    private readonly List<Action<T>> subscribers = new();
    public Type SubscriberType { get; } = typeof(T);

    public void AddSubsciber(Action<T> subscriber)
    {
        this.subscribers.Add(subscriber);
    }

    public void Handle(object message)
    {
        var typedMessage = (T)message;
        var subscribersSpan = CollectionsMarshal.AsSpan(this.subscribers);
        
        for(var i = 0; i<subscribersSpan.Length; i++)
        {
            var subscriber = subscribersSpan[i];
            subscriber(typedMessage);
        }
    }

    public virtual bool IsMatch<TCandidate>()
    {
        return typeof(TCandidate) == SubscriberType;
    }

    internal static Handler<T> Create(bool withAncestors)
    {
        if(withAncestors)
        {
            return new AncestorsHandler<T>();
        }
        return new Handler<T>();
    }
}

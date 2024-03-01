namespace Anthill.Hub;

internal interface IHandler
{
    bool IsMatch<T>();

    void Handle(object message);

    Type SubscriberType { get; }
}

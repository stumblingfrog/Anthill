namespace Anthill.Hub;

internal class AncestorsHandler<T> : Handler<T>
{
    public override bool IsMatch<TCandidate>()
    {
        var candidateType = typeof(TCandidate);
        if(candidateType == SubscriberType)
        {
            return true;
        }
        return candidateType.IsSubclassOf(this.SubscriberType);
    }
}

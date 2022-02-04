namespace Yaub;
using System.Runtime.InteropServices;

public class TrackerCollection : IEnumerable<EntityTracker>
{
    private List<EntityTracker> Trackers { get; } = new(1024);

    public void TerminateAll()
    {
        foreach (var tracker in Trackers)
            tracker.Terminate();
    }

    public void MakeAllWeak()
    {
        foreach (var tracker in Trackers)
            tracker.MakeWeak();
    }

    public EntityTracker GetFreeTracker()
    {
        foreach (var tracker in Trackers)
        {
            if (tracker.IsFree())
                return tracker;
        }

        var result = new EntityTracker();
        Trackers.Add(result);
        return result;
    }

    public IEnumerator<EntityTracker> GetEnumerator()
        => ((IEnumerable<EntityTracker>)Trackers).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
namespace Yaub;

public class EntityTracker
{
    public string? Key { get; set; }
    public ObjectId Id { get; private set; }
    public object? Value { get; private set; }
    public WeakReference? WeakValue { get; private set; }
    public bool IsLocalInstance { get; set; }

    public EntityTracker()
    {

    }

    public EntityTracker(ObjectId id, object value)
    {
        WeakValue = default;
        Id = id;
        Value = value;
    }

    public void Assign(ObjectId id, object value)
    {
        Id = id;
        Value = value;
        WeakValue = null;
        IsLocalInstance = false;
    }

    public object? GetValue()
    {
        if (IsFree())
            return null;

        return Value ?? WeakValue?.Target;
    }

    public void MakeWeak()
    {
        WeakValue = new WeakReference(Value);
        Value = null;
    }

    public void Terminate()
    {
        Value = null;
        WeakValue = null;
        IsLocalInstance = false;
    }

    public bool IsFree()
        => Value == null && WeakValue is not { IsAlive: true };
}
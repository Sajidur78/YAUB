namespace Yaub;

[AttributeUsage(AttributeTargets.Class)]
public class SerializeableAttribute : Attribute
{
}

public interface ISerializable
{
    static abstract string StorageKey { get; }
}
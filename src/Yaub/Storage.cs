namespace Yaub;
using System.Runtime.CompilerServices;
using StorageItem = MongoKeyValuePair<string, object>;

public class Storage
{
    public string Name { get; }
    public IMongoCollection<StorageItem> Items { get; }
    private Dictionary<string, object> Cache { get; } = new();

    public Storage(IMongoDatabase database, string name)
    {
        Name = name;
        Items = database.GetStorageCollection(name);
    }

    public Task<bool> Contains(string key)
        => Cache.ContainsKey(key) ? Task.FromResult(true) : Items.ContainsAsync(key);

    public async Task<T?> Get<T>(string key)
    {
        if (Cache.TryGetValue(key, out var value))
            return (T)value;


        var item = await Items.GetValueAsync(key);
        if (item == null)
            return default;

        var result = (T)item.Value;
        Cache.Add(key, item.Value);
        return result;
    }

    public async Task<T> GetOrCreate<T>(string key) where T : new()
    {
        if (Cache.TryGetValue(key, out var value))
        {
            if (value is not null)
            {
                return (T)value;
            }
        }

        var item = await Items.GetValueAsync(key);

        T result;
        if (item == null)
        {
            result = new T();
            Cache[key] = result;
            return result;
        }

        if (item.Value == null)
        {
            item.Value = new T();
        }

        result = (T)item.Value;
        Cache[key] = item.Value;
        return result;
    }

    public Task Save<T>(T? value) where T: ISerializable
    {
        return Save(T.StorageKey, value);
    }

    public async Task Save(string key, object? value)
    {
        if (value == null)
        {
            await Items.RemoveValueAsync(key);
            Cache.Remove(key);
            return;
        }

        Cache[key] = value;
        await Items.AddOrUpdateAsync(key, value);
    }
}
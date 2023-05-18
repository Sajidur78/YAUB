namespace Yaub;
using System.Runtime.CompilerServices;

using StorageItem = MongoKeyValuePair<string, object>;
public class Storage
{
    public string Name { get; }
    public IMongoCollection<StorageItem> Items { get; }
    private Dictionary<string, object> Cache { get; } = new();
    private TrackerCollection Trackers { get; } = new();
    private ConditionalWeakTable<object, EntityTracker> TrackerLookup { get; } = new();

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
        TrackObject(key, item.Id, ref result);
        Cache.Add(key, item.Value);
        return result;
    }

    public async Task<T> GetOrCreate<T>(string key) where T : new()
    {
        if (Cache.TryGetValue(key, out var value))
            return (T)value;

        var item = await Items.GetValueAsync(key);

        T result;
        if (item == null)
        {
            result = new T();
            Cache.Add(key, result);
            TrackObject(key, ObjectId.GenerateNewId(), ref result, true);
            return result;
        }

        if (item.Value == null)
        {
            item.Value = new T();
        }

        result = (T)item.Value;
        TrackObject(key, item.Id, ref result);
        Cache.Add(key, item.Value);
        return result;
    }

    public async Task Set(string key, object? value)
    {
        if (value == null)
        {
            await Items.RemoveValueAsync(key);
            Cache.Remove(key);
            return;
        }

        if (Cache.ContainsKey(key))
        {
            await Items.AddOrUpdateAsync(key, value);
            var oldCache = Cache[key];
            Cache[key] = value;

            // Re-purpose our old tracker
            if (TrackerLookup.TryGetValue(oldCache, out var tracker))
            {
                tracker.Terminate();
                tracker.Assign(default, value);
                return;
            }

            TrackObject(key, default, ref value);
            return;
        }

        await Items.AddOrUpdateAsync(key, value);
        TrackObject(key, default, ref value);
    }

    public async Task SaveChanges()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var newItems = new List<EntityTracker>();
        var kvp = new MongoKeyValuePair<string, object>(default!, default!);
        foreach (var item in Trackers)
        {
            if (item.IsFree())
                continue;

            if (item.IsLocalInstance)
            {
                newItems.Add(item);
                continue;
            }

            kvp.Key = item.Key!;
            kvp.Value = item.Value!;
            var document = kvp.ToBsonDocument();
            // Exclude id
            document.RemoveAt(0);

            await Items.FindOneAndUpdateAsync(x => x.Id == item.Id, document);
        }

        foreach (var item in newItems)
        {
            kvp.Id = item.Id;
            kvp.Key = item.Key!;
            kvp.Value = item.Value!;
            item.IsLocalInstance = false;
            await Items.InsertOneAsync(kvp);
        }

        Trackers.MakeAllWeak();
        TrackerLookup.Clear();
    }

    private EntityTracker? TrackObject<T>(string key, ObjectId id, ref T value, bool localInstance = false)
    {
        if (typeof(T).IsValueType || value == null)
            return null;
        
        var tracker = Trackers.GetFreeTracker();
        tracker.Assign(id, value);
        tracker.IsLocalInstance = localInstance;
        tracker.Key = key;
        TrackerLookup.Add(value, tracker);

        return tracker;
    }
}
namespace Yaub;
using StorageItem = MongoKeyValuePair<string, object>;

public static class StorageExtensions
{
    public static IMongoCollection<StorageItem> GetStorageCollection(this IMongoDatabase db, string name)
    {
        var collection = db.GetCollection<StorageItem>(name);
        bool hasIndex = false;
        foreach (var idx in collection.Indexes.List().ToEnumerable())
        {
            var idxName = idx["name"].ToString();
            if (idxName == "_key_")
            {
                hasIndex = true;
                break;
            }
        }

        if (!hasIndex)
        {
            collection.Indexes.CreateOne(new CreateIndexModel<StorageItem>("{ k : 1 }", new() { Unique = true, Name = "_key_" }));
        }

        return collection;
    }

    public static async Task<bool> ContainsAsync(this IMongoCollection<StorageItem> collection, string name)
        => await (await collection.FindAsync(x => x.Key == name)).AnyAsync();

    public static async Task<StorageItem?> GetValueAsync(this IMongoCollection<StorageItem> collection, string key)
    {
        var result = await collection.FindAsync(x => x.Key == key);
        if (await result.MoveNextAsync())
            return result.Current.FirstOrDefault();

        return null;
    }

    public static async Task RemoveValueAsync(this IMongoCollection<StorageItem> collection, string key)
    {
        await collection.DeleteOneAsync(x => x.Key == key);
    }

    public static async Task AddOrUpdateAsync(this IMongoCollection<StorageItem> collection, string key, object value)
    {
        if (await collection.ContainsAsync(key))
        {
            await collection.UpdateOneAsync(x => x.Key == key, value.ToBsonDocument());
            return;
        }

        await collection.InsertOneAsync(new(key, value));
    }
}
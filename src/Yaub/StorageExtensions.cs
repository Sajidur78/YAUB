namespace Yaub;
using StorageItem = MongoKeyValuePair<string, object>;

public static class StorageExtensions
{
    public static IMongoCollection<StorageItem> GetStorageCollection(this IMongoDatabase db, string name)
    {
        var collection = db.GetCollection<StorageItem>(name);

        return collection;
    }

    public static async Task<bool> ContainsAsync(this IMongoCollection<StorageItem> collection, string name)
        => await (await collection.FindAsync(x => x.Id == name)).AnyAsync();

    public static async Task<StorageItem?> GetValueAsync(this IMongoCollection<StorageItem> collection, string key)
    {
        var result = await collection.FindAsync(x => x.Id == key);
        if (await result.MoveNextAsync())
            return result.Current.FirstOrDefault();

        return null;
    }

    public static async Task RemoveValueAsync(this IMongoCollection<StorageItem> collection, string key)
    {
        await collection.DeleteOneAsync(x => x.Id == key);
    }

    public static async Task AddOrUpdateAsync(this IMongoCollection<StorageItem> collection, string key, object value)
    {
        if (await collection.ContainsAsync(key))
        {
            await collection.UpdateOneAsync(x => x.Id == key, Builders<StorageItem>.Update.Set(x => x.Value, value));
            return;
        }

        await collection.InsertOneAsync(new(key, value));
    }
}
namespace Yaub;
using MongoDB.Driver;

public class StorageCollection
{
    public MongoClient Client { get; }
    public IMongoDatabase Database { get; }
    private Dictionary<string, Storage> StorageCache { get; } = new();

    public StorageCollection(string connectionString, string database)
    {
        Client = new MongoClient(connectionString);
        Database = Client.GetDatabase(database);
    }

    public Storage Get(string id)
    {
        if (StorageCache.TryGetValue(id, out var value))
            return value;
        
        value = new Storage(Database, id);
        StorageCache.Add(id, value);
        return value;
    }
}
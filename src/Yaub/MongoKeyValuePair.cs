namespace Yaub;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MongoKeyValuePair<TKey, TValue>
{
    public ObjectId Id { get; set; }

    [BsonElement("k")]
    public TKey Key { get; set; }

    [BsonElement("v")]
    public TValue Value { get; set; }

    public MongoKeyValuePair(TKey key, TValue value)
    {
        Id = default;
        Key = key;
        Value = value;
    }
}
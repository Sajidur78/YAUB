namespace Yaub;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MongoKeyValuePair<TKey, TValue>
{
    [BsonId]
    public TKey Id { get; set; }

    [BsonElement("v")]
    public TValue Value { get; set; }

    public MongoKeyValuePair(TKey key, TValue value)
    {
        Id = key;
        Value = value;
    }
}
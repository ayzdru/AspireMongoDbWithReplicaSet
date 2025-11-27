using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AspireMongoDbWithReplicaSet.Core.Entities
{
    public class Movie
    {
        [BsonId]
        public ObjectId _id { get; set; }
        [BsonElement("title")]
        public string Title { get; set; }
        [BsonElement("rated")]
        public string Rated { get; set; }
        [BsonElement("plot")]
        public string Plot { get; set; }
    }
}

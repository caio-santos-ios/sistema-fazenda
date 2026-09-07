using MongoDB.Bson.Serialization.Attributes;

namespace api_bora_trampar.src.Models
{
    public class GroupSubCostCenter : ModelBase
    {
        [BsonElement("code")]
        public string Code { get; set; } = string.Empty;

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("groupCode")]
        public string GroupCode { get; set; } = string.Empty;

        [BsonElement("value")]
        public decimal Value { get; set; } = 0m;
    }
}
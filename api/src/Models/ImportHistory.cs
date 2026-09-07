using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace api_bora_trampar.src.Models
{
    public class ImportHistory : ModelBase
    {
        [BsonElement("fileName")]
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [BsonElement("importedAt")]
        [JsonPropertyName("importedAt")]
        public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("importedBy")]
        [JsonPropertyName("importedBy")]
        public string ImportedBy { get; set; } = string.Empty;

        [BsonElement("importedByName")]
        [JsonPropertyName("importedByName")]
        public string ImportedByName { get; set; } = string.Empty;

        [BsonElement("totalRows")]
        [JsonPropertyName("totalRows")]
        public int TotalRows { get; set; }

        [BsonElement("matchedCount")]
        [JsonPropertyName("matchedCount")]
        public int MatchedCount { get; set; }

        [BsonElement("unmatchedCount")]
        [JsonPropertyName("unmatchedCount")]
        public int UnmatchedCount { get; set; }

        [BsonElement("totalValue")]
        [JsonPropertyName("totalValue")]
        public decimal TotalValue { get; set; }

        [BsonElement("items")]
        [JsonPropertyName("items")]
        public List<ImportHistoryItem> Items { get; set; } = new();
    }

    public class ImportHistoryItem
    {
        [BsonElement("costCenterId")]
        [JsonPropertyName("costCenterId")]
        public string CostCenterId { get; set; } = string.Empty;

        [BsonElement("costCenterCode")]
        [JsonPropertyName("costCenterCode")]
        public string CostCenterCode { get; set; } = string.Empty;

        [BsonElement("costCenterName")]
        [JsonPropertyName("costCenterName")]
        public string CostCenterName { get; set; } = string.Empty;

        [BsonElement("hierarchyPath")]
        [JsonPropertyName("hierarchyPath")]
        public string HierarchyPath { get; set; } = string.Empty;

        [BsonElement("sourceDescription")]
        [JsonPropertyName("sourceDescription")]
        public string SourceDescription { get; set; } = string.Empty;

        [BsonElement("value")]
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }
}

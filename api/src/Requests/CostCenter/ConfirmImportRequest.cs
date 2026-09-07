namespace api_bora_trampar.src.Requests
{
    public class ConfirmCostCenterItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal Value { get; set; } = 0m;
    }

    public class ConfirmImportLineDetailDto
    {
        public string CostCenterId { get; set; } = string.Empty;
        public string CostCenterCode { get; set; } = string.Empty;
        public string CostCenterName { get; set; } = string.Empty;
        public string HierarchyPath { get; set; } = string.Empty;
        public string SourceDescription { get; set; } = string.Empty;
        public decimal Value { get; set; } = 0m;
    }

    public class ConfirmImportRequest
    {
        public string FileName { get; set; } = string.Empty;
        public int TotalRows { get; set; } = 0;
        public int MatchedCount { get; set; } = 0;
        public int UnmatchedCount { get; set; } = 0;
        public decimal TotalValue { get; set; } = 0m;
        public List<ConfirmCostCenterItemDto> Items { get; set; } = new();
        public List<ConfirmImportLineDetailDto> Lines { get; set; } = new();
    }
}

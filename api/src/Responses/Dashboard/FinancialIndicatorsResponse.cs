namespace api_bora_trampar.src.Responses.Dashboard
{
    public class FinancialIndicatorsResponse
    {
        public decimal GrossRevenue { get; set; }
        public decimal COE { get; set; }
        public decimal COT { get; set; }
        public decimal MargemBruta { get; set; }
        public double MargemBrutaPercent { get; set; }
        public decimal EbitdaAgricola { get; set; }
        public double EbitdaPercent { get; set; }
        public decimal CAPEX { get; set; }
        public decimal FCOL { get; set; }
        public decimal CompromissosDividas { get; set; }
        public decimal SaidasNaoOperacionais { get; set; }
        public decimal InvestimentosNaoOperacionais { get; set; }
        public decimal DeltaCaixaFinal { get; set; }
        public List<GroupIndicatorDetail> Groups { get; set; } = [];
    }

    public class GroupIndicatorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CostCenterCount { get; set; }
        public decimal TotalValue { get; set; }
        public double PercentageOfRevenue { get; set; }
    }
}

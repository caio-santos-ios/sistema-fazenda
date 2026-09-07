using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models._Base;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;
using api_bora_trampar.src.Responses.Dashboard;

namespace api_bora_trampar.src.Interfaces
{
    public interface ICostCenterService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllRequest request);
        Task<ResponseApi<CostCenter?>> GetByIdAsync(string id);
        Task<ResponseApi<CostCenter?>> CreateAsync(CreateCostCenterRequest request);
        Task<ResponseApi<CostCenter?>> UpdateAsync(UpdateCostCenterRequest request);
        Task<ResponseApi<CostCenter?>> DeleteAsync(DeleteRequest request);
        Task<ResponseApi<object?>> ImportSpreadsheetAsync(IFormFile file);
        Task<ResponseApi<object?>> ConfirmImportAsync(ConfirmImportRequest request, string userId);
        Task<ResponseApi<CostCenter?>> MoveCostCenterAsync(MoveCostCenterRequest request, string userId);
        Task<ResponseApi<object?>> PreviewMoveCostCenterAsync(string id, string targetGroup, string targetSubGroup, string? targetSubCostCenter);
        Task<ResponseApi<object?>> GetImportHistoriesAsync(int page = 1, int pageSize = 50);
        Task<ResponseApi<ImportHistory?>> GetImportHistoryByIdAsync(string id);
        Task<ResponseApi<object?>> DeleteImportHistoryAsync(string id, string userId);
        Task<ResponseApi<FinancialIndicatorsResponse>> GetFinancialIndicatorsAsync(decimal grossRevenue);
    }
}

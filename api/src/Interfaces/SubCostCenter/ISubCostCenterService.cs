using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models._Base;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Interfaces
{
    public interface ISubCostCenterService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllRequest request);
        Task<ResponseApi<SubCostCenter?>> GetByIdAsync(string id);
        Task<ResponseApi<SubCostCenter?>> CreateAsync(CreateSubCostCenterRequest request);
        Task<ResponseApi<SubCostCenter?>> UpdateAsync(UpdateSubCostCenterRequest request);
        Task<ResponseApi<SubCostCenter?>> DeleteAsync(DeleteRequest request);
    }
}

using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models._Base;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Interfaces
{
    public interface IGroupSubCostCenterService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllRequest request);
        Task<ResponseApi<GroupSubCostCenter?>> GetByIdAsync(string id);
        Task<ResponseApi<GroupSubCostCenter?>> CreateAsync(CreateGroupSubCostCenterRequest request);
        Task<ResponseApi<GroupSubCostCenter?>> UpdateAsync(UpdateGroupSubCostCenterRequest request);
        Task<ResponseApi<GroupSubCostCenter?>> DeleteAsync(DeleteRequest request);
    }
}

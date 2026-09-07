using api_bora_trampar.src.Models;
using api_bora_trampar.src.Models._Base;
using api_bora_trampar.src.Models.Base;
using api_bora_trampar.src.Requests;
using api_bora_trampar.src.Requests._Base;
using api_bora_trampar.src.Requests.Base;

namespace api_bora_trampar.src.Interfaces
{
    public interface IGroupCostCenterService
    {
        Task<ResponseApi<List<dynamic>>> GetAllAsync(GetAllRequest request);
        Task<ResponseApi<List<dynamic>>> GetSelectAsync(GetAllRequest request);
        Task<ResponseApi<GroupCostCenter?>> GetByIdAsync(string id);
        Task<ResponseApi<GroupCostCenter?>> CreateAsync(CreateGroupCostCenterRequest request);
        Task<ResponseApi<GroupCostCenter?>> UpdateAsync(UpdateGroupCostCenterRequest request);
        Task<ResponseApi<GroupCostCenter?>> DeleteAsync(DeleteRequest request);
    }
}

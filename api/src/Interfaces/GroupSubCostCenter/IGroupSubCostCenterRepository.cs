using api_bora_trampar.src.Models;
using MongoDB.Bson;

namespace api_bora_trampar.src.Interfaces
{
    public interface IGroupSubCostCenterRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<long> GetCountAsync(List<BsonDocument> pipeline);
        Task<GroupSubCostCenter?> GetByIdAsync(string id);
        Task<GroupSubCostCenter?> GetByCodeAsync(string code);
        Task<List<GroupSubCostCenter>> GetActiveSubGroupsAsync();
        Task<long> GetCountDocumentsAsync(string group);
        Task<GroupSubCostCenter?> CreateAsync(GroupSubCostCenter entity);
        Task<GroupSubCostCenter?> UpdateAsync(GroupSubCostCenter entity);
        Task<GroupSubCostCenter> DeleteAsync(GroupSubCostCenter entity);
    }
}

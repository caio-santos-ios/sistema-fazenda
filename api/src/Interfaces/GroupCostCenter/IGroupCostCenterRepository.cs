using api_bora_trampar.src.Models;
using MongoDB.Bson;

namespace api_bora_trampar.src.Interfaces
{
    public interface IGroupCostCenterRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<long> GetCountAsync(List<BsonDocument> pipeline);
        Task<GroupCostCenter?> GetByIdAsync(string id);
        Task<GroupCostCenter?> GetByCodeAsync(string code);
        Task<List<GroupCostCenter>> GetActiveGroupsAsync();
        Task<long> GetCountDocumentsAsync();
        Task<GroupCostCenter?> CreateAsync(GroupCostCenter entity);
        Task<GroupCostCenter?> UpdateAsync(GroupCostCenter entity);
        Task<GroupCostCenter> DeleteAsync(GroupCostCenter entity);
    }
}

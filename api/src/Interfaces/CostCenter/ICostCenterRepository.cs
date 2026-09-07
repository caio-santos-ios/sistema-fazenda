using api_bora_trampar.src.Models;
using MongoDB.Bson;

namespace api_bora_trampar.src.Interfaces
{
    public interface ICostCenterRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<long> GetCountAsync(List<BsonDocument> pipeline);
        Task<CostCenter?> GetByIdAsync(string id);
        Task<long> GetCountDocumentsAsync(string group, string subGroup, string subCostCenter);
        Task<CostCenter?> GetByCodeAsync(string code);
        Task<List<CostCenter>> GetActiveCostCentersAsync();
        Task<List<CostCenter>> GetByNameAsync(string name);
        Task<long> BatchUpdateValuesAsync(List<(string Id, decimal Value, string UpdatedBy)> items);
        Task<CostCenter?> CreateAsync(CostCenter entity);
        Task<CostCenter?> UpdateAsync(CostCenter entity);
        Task<CostCenter> DeleteAsync(CostCenter entity);
    }
}

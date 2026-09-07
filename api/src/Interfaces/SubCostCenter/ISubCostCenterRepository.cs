using api_bora_trampar.src.Models;
using MongoDB.Bson;

namespace api_bora_trampar.src.Interfaces
{
    public interface ISubCostCenterRepository
    {
        Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline);
        Task<long> GetCountAsync(List<BsonDocument> pipeline);
        Task<SubCostCenter?> GetByIdAsync(string id);
        Task<SubCostCenter?> GetByCodeAsync(string code);
        Task<List<SubCostCenter>> GetActiveSubCostCentersAsync();
        Task<long> GetCountDocumentsAsync(string group, string subGroup);
        Task<SubCostCenter?> CreateAsync(SubCostCenter entity);
        Task<SubCostCenter?> UpdateAsync(SubCostCenter entity);
        Task<SubCostCenter> DeleteAsync(SubCostCenter entity);
    }
}

using api_bora_trampar.src.Configuration;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_bora_trampar.src.Repositories
{
    public class SubCostCenterRepository(AppDbContext appDbContext) : ISubCostCenterRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.SubCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
        }

        public async Task<long> GetCountAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.SubCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }

        public async Task<SubCostCenter?> GetByIdAsync(string id)
        {
            return await appDbContext.SubCostCenters.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<SubCostCenter?> GetByCodeAsync(string code)
        {
            return await appDbContext.SubCostCenters.Find(x => !x.Deleted && x.Code.Equals(code)).FirstOrDefaultAsync();
        }

        public async Task<List<SubCostCenter>> GetActiveSubCostCentersAsync()
        {
            return await appDbContext.SubCostCenters.Find(x => !x.Deleted).ToListAsync();
        }

        public async Task<long> GetCountDocumentsAsync(string group, string subGroup)
        {
            return await appDbContext.SubCostCenters.Find(x => x.GroupCode == group && x.SubGroupCode == subGroup).CountDocumentsAsync();
        }

        public async Task<SubCostCenter?> CreateAsync(SubCostCenter entity)
        {
            await appDbContext.SubCostCenters.InsertOneAsync(entity);
            return entity;
        }

        public async Task<SubCostCenter?> UpdateAsync(SubCostCenter entity)
        {
            await appDbContext.SubCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }

        public async Task<SubCostCenter> DeleteAsync(SubCostCenter entity)
        {
            await appDbContext.SubCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}

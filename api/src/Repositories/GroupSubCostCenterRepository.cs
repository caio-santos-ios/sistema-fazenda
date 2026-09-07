using api_bora_trampar.src.Configuration;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_bora_trampar.src.Repositories
{
    public class GroupSubCostCenterRepository(AppDbContext appDbContext) : IGroupSubCostCenterRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.GroupSubCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
        }

        public async Task<long> GetCountAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.GroupSubCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }

        public async Task<GroupSubCostCenter?> GetByIdAsync(string id)
        {
            return await appDbContext.GroupSubCostCenters.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<GroupSubCostCenter?> GetByCodeAsync(string code)
        {
            return await appDbContext.GroupSubCostCenters.Find(x => !x.Deleted && x.Code.Equals(code)).FirstOrDefaultAsync();
        }

        public async Task<List<GroupSubCostCenter>> GetActiveSubGroupsAsync()
        {
            return await appDbContext.GroupSubCostCenters.Find(x => !x.Deleted).ToListAsync();
        }

        public async Task<long> GetCountDocumentsAsync(string group)
        {
            return await appDbContext.GroupSubCostCenters.Find(x => x.GroupCode == group).CountDocumentsAsync();
        }

        public async Task<GroupSubCostCenter?> CreateAsync(GroupSubCostCenter entity)
        {
            await appDbContext.GroupSubCostCenters.InsertOneAsync(entity);
            return entity;
        }

        public async Task<GroupSubCostCenter?> UpdateAsync(GroupSubCostCenter entity)
        {
            await appDbContext.GroupSubCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }

        public async Task<GroupSubCostCenter> DeleteAsync(GroupSubCostCenter entity)
        {
            await appDbContext.GroupSubCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}

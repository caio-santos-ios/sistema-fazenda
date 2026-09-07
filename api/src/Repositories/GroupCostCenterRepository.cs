using api_bora_trampar.src.Configuration;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_bora_trampar.src.Repositories
{
    public class GroupCostCenterRepository(AppDbContext appDbContext) : IGroupCostCenterRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.GroupCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
        }

        public async Task<long> GetCountAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.GroupCostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }

        public async Task<GroupCostCenter?> GetByIdAsync(string id)
        {
            return await appDbContext.GroupCostCenters.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<GroupCostCenter?> GetByCodeAsync(string code)
        {
            return await appDbContext.GroupCostCenters.Find(x => !x.Deleted && x.Code.Equals(code)).FirstOrDefaultAsync();
        }

        public async Task<List<GroupCostCenter>> GetActiveGroupsAsync()
        {
            return await appDbContext.GroupCostCenters.Find(x => !x.Deleted).ToListAsync();
        }

        public async Task<long> GetCountDocumentsAsync()
        {
            return await appDbContext.GroupCostCenters.Find(x => true).CountDocumentsAsync();
        }

        public async Task<GroupCostCenter?> CreateAsync(GroupCostCenter entity)
        {
            await appDbContext.GroupCostCenters.InsertOneAsync(entity);
            return entity;
        }

        public async Task<GroupCostCenter?> UpdateAsync(GroupCostCenter entity)
        {
            await appDbContext.GroupCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }

        public async Task<GroupCostCenter> DeleteAsync(GroupCostCenter entity)
        {
            await appDbContext.GroupCostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}

using api_bora_trampar.src.Configuration;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace api_bora_trampar.src.Repositories
{
    public class CostCenterRepository(AppDbContext appDbContext) : ICostCenterRepository
    {
        public async Task<List<dynamic>> GetAllAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.CostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).ToList();
        }

        public async Task<long> GetCountAsync(List<BsonDocument> pipeline)
        {
            var results = await appDbContext.CostCenters.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return results.Select(doc => BsonSerializer.Deserialize<dynamic>(doc)).Count();
        }

        public async Task<CostCenter?> GetByIdAsync(string id)
        {
            return await appDbContext.CostCenters.Find(x => !x.Deleted && x.Id.Equals(id)).FirstOrDefaultAsync();
        }

        public async Task<CostCenter?> GetByCodeAsync(string code)
        {
            return await appDbContext.CostCenters.Find(x => !x.Deleted && x.Code.Equals(code)).FirstOrDefaultAsync();
        }

        public async Task<List<CostCenter>> GetActiveCostCentersAsync()
        {
            return await appDbContext.CostCenters.Find(x => !x.Deleted).ToListAsync();
        }
        public async Task<List<CostCenter>> GetByNameAsync(string name)
        {
            return await appDbContext.CostCenters.Find(x => !x.Deleted && x.Name.ToLower().Contains(name.ToLower())).ToListAsync();
        }

        public async Task<long> BatchUpdateValuesAsync(List<(string Id, decimal Value, string UpdatedBy)> items)
        {
            if (items == null || items.Count == 0) return 0;

            var updates = new List<WriteModel<CostCenter>>();
            foreach (var item in items)
            {
                var filter = Builders<CostCenter>.Filter.And(
                    Builders<CostCenter>.Filter.Eq(x => x.Id, item.Id),
                    Builders<CostCenter>.Filter.Eq(x => x.Deleted, false)
                );

                var update = Builders<CostCenter>.Update
                    .Set(x => x.Value, item.Value)
                    .Set(x => x.UpdatedAt, DateTime.UtcNow)
                    .Set(x => x.UpdatedBy, item.UpdatedBy);

                updates.Add(new UpdateOneModel<CostCenter>(filter, update));
            }

            var result = await appDbContext.CostCenters.BulkWriteAsync(updates);
            return result.ModifiedCount;
        }

        public async Task<long> GetCountDocumentsAsync(string group, string subGroup, string subCostCenter)
        {
            if (string.IsNullOrWhiteSpace(subCostCenter))
            {
                return await appDbContext.CostCenters.Find(x => x.GroupCode == group && x.SubGroupCode == subGroup && (x.SubCostCenter == null || x.SubCostCenter == "")).CountDocumentsAsync();
            }
            return await appDbContext.CostCenters.Find(x => x.GroupCode == group && x.SubGroupCode == subGroup && x.SubCostCenter == subCostCenter).CountDocumentsAsync();
        }

        public async Task<CostCenter?> CreateAsync(CostCenter entity)
        {
            await appDbContext.CostCenters.InsertOneAsync(entity);
            return entity;
        }

        public async Task<CostCenter?> UpdateAsync(CostCenter entity)
        {
            await appDbContext.CostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }

        public async Task<CostCenter> DeleteAsync(CostCenter entity)
        {
            await appDbContext.CostCenters.ReplaceOneAsync(x => x.Id.Equals(entity.Id), entity);
            return entity;
        }
    }
}

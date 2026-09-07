using api_bora_trampar.src.Configuration;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Models;
using MongoDB.Driver;

namespace api_bora_trampar.src.Repositories
{
    public class ImportHistoryRepository : IImportHistoryRepository
    {
        private readonly AppDbContext _context;

        public ImportHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportHistory> CreateAsync(ImportHistory history)
        {
            history.CreatedAt = DateTime.UtcNow;
            history.UpdatedAt = DateTime.UtcNow;
            history.ImportedAt = DateTime.UtcNow;
            await _context.ImportHistories.InsertOneAsync(history);
            return history;
        }

        public async Task<List<ImportHistory>> GetAllAsync(int page = 1, int pageSize = 50)
        {
            int skip = Math.Max(0, (page - 1) * pageSize);
            return await _context.ImportHistories
                .Find(x => !x.Deleted)
                .SortByDescending(x => x.ImportedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<long> GetCountAsync()
        {
            return await _context.ImportHistories
                .Find(x => !x.Deleted)
                .CountDocumentsAsync();
        }

        public async Task<ImportHistory?> GetByIdAsync(string id)
        {
            return await _context.ImportHistories
                .Find(x => x.Id == id && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<ImportHistory>.Update
                .Set(x => x.Deleted, true)
                .Set(x => x.UpdatedAt, DateTime.UtcNow);
            var result = await _context.ImportHistories.UpdateOneAsync(x => x.Id == id, update);
            return result.ModifiedCount > 0;
        }
    }
}

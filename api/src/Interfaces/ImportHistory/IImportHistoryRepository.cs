using api_bora_trampar.src.Models;

namespace api_bora_trampar.src.Interfaces
{
    public interface IImportHistoryRepository
    {
        Task<ImportHistory> CreateAsync(ImportHistory history);
        Task<List<ImportHistory>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<long> GetCountAsync();
        Task<ImportHistory?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
    }
}

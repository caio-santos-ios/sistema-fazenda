using api_bora_trampar.src.Models;
using MongoDB.Driver;

namespace api_bora_trampar.src.Configuration
{
    public class AppDbContext
    {
        private IMongoDatabase Database { get; }

        public AppDbContext()
        {
            try
            {
                string connectionString = Environment.GetEnvironmentVariable("MONGODB_CONNECTION") ?? "";
                string databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME") ?? "";
                MongoClient client = new (connectionString);

                Database = client.GetDatabase(databaseName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to database. Error: {ex.Message}");
            }
        }

        public IMongoCollection<User> Users => Database.GetCollection<User>("users");
        public IMongoCollection<GroupCostCenter> GroupCostCenters => Database.GetCollection<GroupCostCenter>("group_cost_centers");
        public IMongoCollection<GroupSubCostCenter> GroupSubCostCenters => Database.GetCollection<GroupSubCostCenter>("group_sub_cost_centers");
        public IMongoCollection<SubCostCenter> SubCostCenters => Database.GetCollection<SubCostCenter>("sub_cost_centers");
        public IMongoCollection<CostCenter> CostCenters => Database.GetCollection<CostCenter>("cost_centers");
        public IMongoCollection<ImportHistory> ImportHistories => Database.GetCollection<ImportHistory>("import_histories");
    }
}

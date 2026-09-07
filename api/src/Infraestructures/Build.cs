using api_bora_trampar.src.Handlers;
using api_bora_trampar.src.Interfaces;
using api_bora_trampar.src.Interfaces.Auth;
using api_bora_trampar.src.Repositories;
using api_bora_trampar.src.Services;

namespace api_bora_trampar.src.Configuration
{
    public static class Build
    {
        public static void AddContext(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<AppDbContext>();
        }

        public static void AddBuilderServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IAuthService, AuthService>();
            builder.Services.AddTransient<IAuthRepository, AuthRepository>();

            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();

            builder.Services.AddTransient<IGroupCostCenterService, GroupCostCenterService>();
            builder.Services.AddTransient<IGroupCostCenterRepository, GroupCostCenterRepository>();

            builder.Services.AddTransient<IGroupSubCostCenterService, GroupSubCostCenterService>();
            builder.Services.AddTransient<IGroupSubCostCenterRepository, GroupSubCostCenterRepository>();

            builder.Services.AddTransient<ISubCostCenterService, SubCostCenterService>();
            builder.Services.AddTransient<ISubCostCenterRepository, SubCostCenterRepository>();

            builder.Services.AddTransient<ICostCenterService, CostCenterService>();
            builder.Services.AddTransient<ICostCenterRepository, CostCenterRepository>();

            builder.Services.AddTransient<IImportHistoryRepository, ImportHistoryRepository>();

            builder.Services.AddTransient<MailHandler>();
        }
    }
}
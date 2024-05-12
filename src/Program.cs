
using MedicLaunchApi.Data;
using MedicLaunchApi.Models;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using MedicLaunchApi.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MedicLaunchApi
{
    public class Program
    {
        private const string LocalDevCorsPolicy = "LocalDevelopmentCorsPolicy";
        private const string ProdCorsPolicy = "ProdCorsPolicy";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, options => options.UseAzureSqlDefaults()));

            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<MedicLaunchUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<UserManager<MedicLaunchUser>>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<QuestionRepositoryLegacy>();
            builder.Services.AddScoped<AzureBlobClient>();
            builder.Services.AddScoped<PracticeService>();
            builder.Services.AddScoped<PaymentRepository>();
            builder.Services.AddScoped<UserDataRepository>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<QuestionRepository>();
            builder.Services.AddScoped<FlashcardRepository>();

            var services = builder.Services;
            services.AddCors(options =>
            {
                options.AddPolicy(ProdCorsPolicy,
                    policy =>
                    {
                        policy.AllowAnyHeader()
                            .AllowCredentials()
                            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                            .WithOrigins("https://mediclaunchapi.azurewebsites.net", "https://mediclaunchdb.z13.web.core.windows.net", "https://mediclaunch.azureedge.net");
                    });

                options.AddPolicy(LocalDevCorsPolicy,
                        policy =>
                        {
                            policy.AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin();
                        });
            });


            var app = builder.Build();
            app.MapIdentityApi<MedicLaunchUser>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            // TODO: Enable this
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseCors(LocalDevCorsPolicy);
            //}
            //else
            //{
            //    app.UseCors(ProdCorsPolicy);
            //}
            app.UseCors(LocalDevCorsPolicy);

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

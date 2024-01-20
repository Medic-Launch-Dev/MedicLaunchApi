
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

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, options => options.UseAzureSqlDefaults()));

            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<MedicLaunchUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<UserManager<MedicLaunchUser>>();
                //.AddSignInManager<UserManager<MedicLaunchUser>>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<QuestionRepository>();
            builder.Services.AddScoped<AzureBlobClient>();
            builder.Services.AddScoped<PracticeService>();

            var services = builder.Services;
            services.AddCors(options =>
            {
                options.AddPolicy(ProdCorsPolicy,
                    policy =>
                    {
                        policy.AllowAnyHeader()
                            .AllowCredentials()
                            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                            .WithOrigins("https://mediclaunchapi.azurewebsites.net", "https://mediclaunchdb.z13.web.core.windows.net");
                    });
            });

            // add local development CORS policy for local development environment
            services.AddCors(options =>
            {
                options.AddPolicy(LocalDevCorsPolicy,
                                       policy =>
                                       {
                                           policy.AllowAnyHeader()
                                               .AllowCredentials()
                                               .AllowAnyMethod()
                            //.AllowAnyOrigin();
                                               .WithOrigins("https://mediclaunchapi.azurewebsites.net", "https://mediclaunchdb.z13.web.core.windows.net", "http://localhost:3000", "https://localhost:3000");
                                       });
            });


            var app = builder.Build();
            app.MapIdentityApi<MedicLaunchUser>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

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

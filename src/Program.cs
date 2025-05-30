using MedicLaunchApi.Authorization;
using Microsoft.AspNetCore.Authorization;
using MedicLaunchApi.Data;
using MedicLaunchApi.Models;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Services;
using MedicLaunchApi.Storage;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using MedicLaunchApi.Configurations;

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

            builder.Services.AddIdentityApiEndpoints<MedicLaunchUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<UserManager<MedicLaunchUser>>()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
            });

            builder.Services.AddAuthorization(options =>
            {
                // Admin should be able to do whatever other roles can do
                options.AddPolicy(RoleConstants.Admin, policy => policy.RequireRole(RoleConstants.Admin));
                options.AddPolicy(RoleConstants.Student, policy => policy.RequireRole(RoleConstants.Student, RoleConstants.Admin));
                options.AddPolicy(RoleConstants.QuestionAuthor, policy => policy.RequireRole(RoleConstants.QuestionAuthor, RoleConstants.Admin));
                options.AddPolicy(RoleConstants.FlashcardAuthor, policy => policy.RequireRole(RoleConstants.FlashcardAuthor, RoleConstants.Admin));
                options.AddPolicy(AuthPolicies.RequireSubscriptionOrTrial, policy => policy.Requirements.Add(new SubscriptionOrTrialRequirement()));
            });

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<IAzureBlobClient, AzureBlobClient>();

            builder.Services.AddScoped<QuestionRepositoryLegacy>();
            builder.Services.AddScoped<AzureBlobClient>();
            builder.Services.AddScoped<PaymentRepository>();
            builder.Services.AddScoped<UserDataRepository>();
            builder.Services.AddScoped<QuestionRepository>();
            builder.Services.AddScoped<FlashcardRepository>();
            builder.Services.AddScoped<NotificationRepository>();
            builder.Services.AddScoped<MockExamRepository>();
            builder.Services.AddScoped<CoursesRepository>();
            builder.Services.AddScoped<TextbookLessonRepository>();
            builder.Services.AddScoped<ClinicalCaseRepository>();

            builder.Services.AddScoped<IEmailSender<MedicLaunchUser>, EmailSender<MedicLaunchUser>>();
            builder.Services.AddSingleton<IEmailSender<MedicLaunchUser>, EmailSender<MedicLaunchUser>>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<IMixPanelService, MixPanelService>();
            builder.Services.AddScoped<AzureOpenAIService>();
            builder.Services.AddScoped<OpenAIService>();
            builder.Services.AddScoped<TextbookLessonGenerationService>();
            builder.Services.AddScoped<IQuestionGenerationService, QuestionGenerationService>();
            builder.Services.AddScoped<ClinicalCaseService>();
            builder.Services.AddScoped<IAuthorizationHandler, SubscriptionOrTrialRequirementHandler>();

            builder.Services.AddOptions<BearerTokenOptions>(IdentityConstants.BearerScheme).Configure(options =>
            {
                options.BearerTokenExpiration = TimeSpan.FromHours(24);
            });

            var services = builder.Services;

            services.AddCors(options =>
            {
                options.AddPolicy(ProdCorsPolicy,
                    policy =>
                    {
                        policy.AllowAnyHeader()
                            .AllowCredentials()
                            .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
                            .WithOrigins("https://mediclaunchapi.azurewebsites.net", "https://mediclaunchdb.z13.web.core.windows.net", "https://mediclaunch.azureedge.net", "https://mediclaunchprod.azureedge.net");
                    });

                options.AddPolicy(LocalDevCorsPolicy,
                        policy =>
                        {
                            policy.AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin();
                        });
            });

            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    if (httpContext.User.Identity?.IsAuthenticated == true &&
                        httpContext.User.IsInRole(RoleConstants.Admin))
                    {
                        return RateLimitPartition.GetNoLimiter("admin-exempt");
                    }

                    var user = httpContext.User.Identity?.Name;
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString();
                    var partitionKey = !string.IsNullOrEmpty(user) ? user : ip ?? "unknown";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    });
                });

                options.AddPolicy(RateLimitingPolicies.Strict, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 4,
                            Window = TimeSpan.FromMinutes(0.5),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }
                    ));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", cancellationToken: token);
                };
            });

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddAzureWebAppDiagnostics();
            });


            var app = builder.Build();
            app.MapCustomIdentityApi<MedicLaunchUser>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseCors(LocalDevCorsPolicy);

            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            SeedRoles(builder.Services).GetAwaiter().GetResult();

            app.Run();
        }

        private static async Task SeedRoles(IServiceCollection services)
        {
            RoleManager<IdentityRole> roleManager = services.BuildServiceProvider().GetRequiredService<RoleManager<IdentityRole>>();

            var existingRoles = await roleManager.Roles.ToListAsync();
            var roles = new List<IdentityRole>
            {
                new IdentityRole { Name = RoleConstants.Admin },
                new IdentityRole { Name = RoleConstants.Student },
                new IdentityRole { Name = RoleConstants.QuestionAuthor },
                new IdentityRole { Name = RoleConstants.FlashcardAuthor }
            };

            foreach (var role in roles)
            {
                if (!existingRoles.Any(r => r.Name == role.Name))
                {
                    await roleManager.CreateAsync(role);
                }
            }

            var userManager = services.BuildServiceProvider().GetRequiredService<UserManager<MedicLaunchUser>>();
            var adminUsers = new string[] { "sajjaadkhalil@gmail.com", "khalid.abdilahi91@gmail.com", "wryhook@gmail.com", "admin@mediclaunch.com" };
            foreach (var email in adminUsers)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await userManager.AddToRoleAsync(user, RoleConstants.Admin);
                }
            }
        }
    }
}

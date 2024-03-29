using FlueFlame.AspNetCore;
using FlueFlame.Http.Host;
using FlueFlame.Serialization.Newtonsoft;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stockband.Domain.Enums;
using Stockband.Domain.Exceptions;
using Stockband.Infrastructure;
using Stockband.Infrastructure.Services;
namespace Stockband.Api.E2E;

public abstract class BaseTest
{
    protected TestServer TestServer { get; set; }
    protected IFlueFlameHttpHost HttpHost { get; set; }
    protected IServiceProvider ServiceProvider { get; set; }
    
    protected StockbandDbContext Context => ServiceProvider.CreateScope()
        .ServiceProvider.GetRequiredService<StockbandDbContext>();

    [SetUp]
    protected async Task Setup()
    {
        WebApplicationFactory<Program> factory =
            new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("E2E");
                    
                    builder.ConfigureServices(services =>
                    {
                        ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(
                            d => d.ServiceType ==
                                 typeof(DbContextOptions<StockbandDbContext>));
                        if (dbContextDescriptor == null)
                        {
                            throw new ObjectNotFound(typeof(ServiceDescriptor), typeof(DbContextOptions<StockbandDbContext>));
                        }
                        
                        services.Remove(dbContextDescriptor);

                        string dbName = $"E2E_{Guid.NewGuid()}";
                        services.AddDbContext<StockbandDbContext>(x => x.UseInMemoryDatabase(dbName));
                    });
                });

        TestServer = factory.Server;
        ServiceProvider = factory.Services;
        
        HttpHost = FlueFlameAspNetBuilder.CreateDefaultBuilder(factory)
            .BuildHttpHost(builder =>
            {
                builder.UseNewtonsoftJsonSerializer();
                builder.ConfigureHttpClient(configure =>
                {
                    configure.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
                });
                builder.Build();
            });

        await Context.Database.EnsureCreatedAsync();
    }
    
    [TearDown]
    protected async Task CleanUp()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
        
        TestServer.Dispose();
    }

    protected string GetUserJwtToken
        (int userId, string username = "user", string email = "user@gmail.com")
    {
        return GetJwtToken(userId, username, email, UserRole.User);
    }

    protected string GetAdminJwtToken
        (int userId, string username = "admin", string email = "admin@gmail.com")
    {
        return GetJwtToken(userId, username, email, UserRole.Admin);
    }
    
    
    private string GetJwtToken(int userId, string username, string email, UserRole userRole)
    {
        IConfiguration configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        IHttpContextAccessor httpContextAccessor = ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        
        ConfigurationHelperService configurationHelper = 
            new ConfigurationHelperService(configuration);

        AuthenticationUserService authenticationUser =
            new AuthenticationUserService(httpContextAccessor, configurationHelper);

        string token =  authenticationUser.GetAccessToken(userId, username, email, userRole);

        return token;
    }
}
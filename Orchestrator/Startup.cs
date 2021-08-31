using System.Net.Http;
using Amazon.S3;
using DLCS.Model.Assets;
using DLCS.Model.Customer;
using DLCS.Model.PathElements;
using DLCS.Model.Security;
using DLCS.Model.Storage;
using DLCS.Repository;
using DLCS.Repository.Assets;
using DLCS.Repository.Caching;
using DLCS.Repository.Customers;
using DLCS.Repository.Security;
using DLCS.Repository.Settings;
using DLCS.Repository.Storage.S3;
using DLCS.Repository.Strategy;
using DLCS.Repository.Strategy.DependencyInjection;
using DLCS.Repository.Strategy.Utils;
using DLCS.Web.Configuration;
using DLCS.Web.Middleware;
using DLCS.Web.Requests.AssetDelivery;
using DLCS.Web.Response;
using LazyCache;
using LazyCache.Providers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orchestrator.Assets;
using Orchestrator.Features.Images;
using Orchestrator.Features.Images.Orchestration;
using Orchestrator.Features.Images.Orchestration.Status;
using Orchestrator.Features.TimeBased;
using Orchestrator.Infrastructure;
using Orchestrator.Infrastructure.Mediatr;
using Orchestrator.Infrastructure.ReverseProxy;
using Orchestrator.Settings;
using Serilog;

namespace Orchestrator
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var reverseProxySection = configuration.GetSection("ReverseProxy");
            var cachingSection = configuration.GetSection("Caching");
            services
                .Configure<OrchestratorSettings>(configuration)
                .Configure<ThumbsSettings>(configuration.GetSection("Thumbs"))
                .Configure<ProxySettings>(configuration.GetSection("Proxy"))
                .Configure<CacheSettings>(cachingSection)
                .Configure<ReverseProxySettings>(reverseProxySection);
            
            var cacheSettings = cachingSection.Get<CacheSettings>();
            services
                .AddMemoryCache(memoryCacheOptions =>
                {
                    memoryCacheOptions.SizeLimit = cacheSettings.MemoryCacheSizeLimit;
                    memoryCacheOptions.CompactionPercentage = cacheSettings.MemoryCacheCompactionPercentage;
                })
                .AddLazyCache()
                .AddSingleton<ICustomerRepository, CustomerRepository>()
                .AddSingleton<IPathCustomerRepository, CustomerPathElementRepository>()
                .AddSingleton<IAssetRepository, DapperAssetRepository>()
                .AddSingleton<IAssetDeliveryPathParser, AssetDeliveryPathParser>()
                .AddSingleton<ImageRequestHandler>()
                .AddSingleton<TimeBasedRequestHandler>()
                .AddAWSService<IAmazonS3>()
                .AddSingleton<IBucketReader, BucketReader>()
                .AddSingleton<IThumbReorganiser, NonOrganisingReorganiser>()
                .AddSingleton<IThumbRepository, ThumbRepository>()
                .AddSingleton<IAssetTracker, MemoryAssetTracker>()
                .AddSingleton<ICredentialsRepository, DapperCredentialsRepository>()
                .AddSingleton<IAuthServicesRepository, DapperAuthServicesRepository>()
                .AddScoped<ICustomerOriginStrategyRepository, CustomerOriginStrategyRepository>()
                .AddSingleton<IImageOrchestrator, ImageOrchestrator>()
                .AddSingleton<IImageOrchestrationStatusProvider, FileBasedStatusProvider>()
                .AddTransient<IAssetPathGenerator, ConfigDrivenAssetPathGenerator>()
                .AddOriginStrategies()
                .AddDbContext<DlcsContext>(opts =>
                    opts.UseNpgsql(configuration.GetConnectionString("PostgreSQLConnection"))
                )
                .AddMediatR()
                .AddHttpContextAccessor();

            var baseAddress = reverseProxySection.Get<ReverseProxySettings>()
                .GetAddressForProxyTarget(ProxyDestination.Orchestrator);
            services
                .AddHttpClient<IDeliveratorClient, DeliveratorClient>(client =>
                {
                    client.DefaultRequestHeaders.WithRequestedBy();
                    client.BaseAddress = baseAddress;
                })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseCookies = false });
            
            // Use x-forwarded-host and x-forwarded-proto to set httpContext.Request.Host and .Scheme respectively
            services.Configure<ForwardedHeadersOptions>(opts =>
            {
                opts.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddControllers()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);
            
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            
            DapperMappings.Register();
            
            services
                .AddHealthChecks()
                .AddNpgSql(configuration.GetPostgresSqlConnection());
            
            // Add the reverse proxy to capability to the server
            services
                .AddReverseProxy()
                .LoadFromConfig(reverseProxySection);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var applicationOptions = configuration.Get<OrchestratorSettings>();
            var pathBase = applicationOptions.PathBase;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app
                .HandlePathBase(pathBase, logger)
                .UseForwardedHeaders()
                .UseHttpsRedirection()
                .UseRouting()
                .UseSerilogRequestLogging()
                .UseCors("CorsPolicy")
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapReverseProxy();
                    endpoints.MapImageHandling();
                    endpoints.MapTimeBasedHandling();
                    endpoints.MapHealthChecks("/health");
                });
        }
    }
}
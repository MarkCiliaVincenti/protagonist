using DLCS.AWS.Configuration;
using DLCS.AWS.S3;
using DLCS.AWS.SQS;
using DLCS.Model.Assets;
using DLCS.Model.Assets.Thumbs;
using DLCS.Model.Customers;
using DLCS.Model.Policies;
using DLCS.Repository;
using DLCS.Repository.Assets.Thumbs;
using DLCS.Repository.Caching;
using DLCS.Repository.Customers;
using DLCS.Repository.Policies;
using DLCS.Repository.Strategy;
using DLCS.Repository.Strategy.DependencyInjection;
using Engine.Ingest;
using Engine.Ingest.Handlers;
using Engine.Ingest.Image;
using Engine.Ingest.Workers;
using Engine.Messaging;
using Engine.Settings;

namespace Engine.Infrastructure;

public static class ServiceCollectionX
{
    /// <summary>
    /// Add required AWS services
    /// </summary>
    public static IServiceCollection AddAws(this IServiceCollection services,
        IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        services
            .AddSingleton<IBucketReader, S3BucketReader>()
            .AddSingleton<IBucketWriter, S3BucketWriter>()
            .AddSingleton<IStorageKeyGenerator, S3StorageKeyGenerator>()
            .SetupAWS(configuration, webHostEnvironment)
            .WithAmazonS3()
            .WithAmazonSQS();

        return services;
    }

    /// <summary>
    /// Configure listeners for queues
    /// </summary>
    public static IServiceCollection AddQueueMonitoring(this IServiceCollection services)
        => services
            .AddSingleton<SqsListenerManager>()
            .AddTransient(typeof(SqsListener<>))
            .AddSingleton<QueueHandlerResolver<EngineMessageType>>(provider => messageType => messageType switch
            {
                EngineMessageType.Ingest => provider.GetRequiredService<IngestHandler>(),
                EngineMessageType.TranscodeComplete => provider.GetRequiredService<TranscodeCompletionHandler>(),
                _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
            })
            .AddTransient<IngestHandler>()
            .AddTransient<TranscodeCompletionHandler>()
            .AddSingleton<SqsQueueUtilities>()
            .AddHostedService<SqsListenerService>();

    /// <summary>
    /// Adds all asset ingestion classes and related dependencies. 
    /// </summary>
    public static IServiceCollection AddAssetIngestion(this IServiceCollection services, EngineSettings engineSettings)
    {
        services
            .AddScoped<IAssetIngester, AssetIngester>()
            .AddTransient<ImageIngesterWorker>()
            .AddSingleton<IThumbLayoutManager, ThumbLayoutManager>()
            .AddTransient<IngestorResolver>(provider => family => family switch
            {
                AssetFamily.Image => provider.GetRequiredService<ImageIngesterWorker>(),
                AssetFamily.Timebased => throw new NotImplementedException("Not yet"),
                AssetFamily.File => throw new NotImplementedException("File shouldn't be here"),
                _ => throw new KeyNotFoundException("Attempt to resolve ingestor handler for unknown family")
            })
            .AddScoped<AssetToDisk>()
            //.AddScoped<AssetToS3>()
            .AddTransient<AssetMoverResolver>(provider => t => t switch
            {
                AssetMoveType.Disk => provider.GetRequiredService<AssetToDisk>(),
                AssetMoveType.ObjectStore => throw new NotImplementedException("Not yet"),
                // AssetMoveType.ObjectStore => provider.GetService<AssetToS3>(),
                _ => throw new NotImplementedException()
            })
            .AddOriginStrategies();

        services.AddHttpClient<IImageProcessor, AppetiserClient>(client =>
        {
            client.BaseAddress = engineSettings.ImageIngest.ImageProcessorUrl;
            client.Timeout = TimeSpan.FromMilliseconds(engineSettings.ImageIngest.ImageProcessorTimeoutMs);
        });

        return services;
    }

    /// <summary>
    /// Add all dataaccess dependencies, including repositories and DLCS context 
    /// </summary>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddScoped<IPolicyRepository, PolicyRepository>()
            .AddScoped<ICustomerOriginStrategyRepository, CustomerOriginStrategyRepository>()
            .AddDlcsContext(configuration);

    /// <summary>
    /// Add required caching dependencies
    /// </summary>
    public static IServiceCollection AddCaching(this IServiceCollection services, CacheSettings cacheSettings)
        => services
            .AddMemoryCache(memoryCacheOptions =>
            {
                memoryCacheOptions.SizeLimit = cacheSettings.MemoryCacheSizeLimit;
                memoryCacheOptions.CompactionPercentage = cacheSettings.MemoryCacheCompactionPercentage;
            })
            .AddLazyCache();

    /// <summary>
    /// Add HealthChecks for Database and Queues
    /// </summary>
    public static IServiceCollection ConfigureHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<DlcsContext>("DLCS-DB")
            .AddQueueHealthCheck();

        return services;
    }
}
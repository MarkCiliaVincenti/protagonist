﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DLCS.Core.Types;
using DLCS.Model.Assets;
using DLCS.Repository.Caching;
using LazyCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace DLCS.Repository.Assets;

/// <summary>
/// Implementation of <see cref="IAssetRepository"/> using Dapper for data access.
/// </summary>
public class DapperAssetRepository : DapperRepository, IAssetRepository
{
    private readonly CacheSettings cacheSettings;
    private readonly IAppCache appCache;
    private readonly ILogger<DapperAssetRepository> logger;
    private static readonly Asset NullAsset = new() { Id = "__nullasset__" };

    public DapperAssetRepository(
        IConfiguration configuration, 
        IAppCache appCache,
        IOptions<CacheSettings> cacheOptions,
        ILogger<DapperAssetRepository> logger) : base(configuration)
    {
        this.appCache = appCache;
        this.logger = logger;
        cacheSettings = cacheOptions.Value;
    }
    
    public Task<Asset?> GetAsset(string id) => GetAsset(id, false);

    public Task<Asset?> GetAsset(AssetId id) => GetAsset(id, false);

    public async Task<Asset?> GetAsset(string id, bool noCache)
    {
        var asset = await GetAssetInternal(id, noCache);
        return asset.Id == NullAsset.Id ? null : asset;
    }

    public Task<Asset?> GetAsset(AssetId id, bool noCache)
        => GetAsset(id.ToString(), noCache);

    public async Task<ImageLocation> GetImageLocation(AssetId assetId) 
        => await QuerySingleOrDefaultAsync<ImageLocation>(ImageLocationSql, new {Id = assetId.ToString()});

    private async Task<Asset> GetAssetInternal(string id, bool noCache = false)
    {
        var key = $"asset:{id}";
        if (noCache)
        {
            appCache.Remove(key);
        }
        return await appCache.GetOrAddAsync(key, async entry =>
        {
            logger.LogDebug("Refreshing assetCache from database {Asset}", id);
            dynamic? rawAsset = await QuerySingleOrDefaultAsync(AssetSql, new { Id = id });
            if (rawAsset == null)
            {
                entry.AbsoluteExpirationRelativeToNow =
                    TimeSpan.FromSeconds(cacheSettings.GetTtl(CacheDuration.Short));
                return NullAsset;
            }

            return new Asset
            {
                Batch = rawAsset.Batch,
                Created = rawAsset.Created,
                Customer = rawAsset.Customer,
                Duration = rawAsset.Duration,
                Error = rawAsset.Error,
                Family = (AssetFamily)rawAsset.Family.ToString()[0],
                Finished = rawAsset.Finished,
                Height = rawAsset.Height,
                Id = rawAsset.Id,
                Ingesting = rawAsset.Ingesting,
                Origin = rawAsset.Origin,
                Reference1 = rawAsset.Reference1,
                Reference2 = rawAsset.Reference2,
                Reference3 = rawAsset.Reference3,
                Roles = rawAsset.Roles,
                Space = rawAsset.Space,
                Tags = rawAsset.Tags,
                Width = rawAsset.Width,
                MaxUnauthorised = rawAsset.MaxUnauthorised,
                MediaType = rawAsset.MediaType,
                NumberReference1 = rawAsset.NumberReference1,
                NumberReference2 = rawAsset.NumberReference2,
                NumberReference3 = rawAsset.NumberReference3,
                PreservedUri = rawAsset.PreservedUri,
                ThumbnailPolicy = rawAsset.ThumbnailPolicy,
                ImageOptimisationPolicy = rawAsset.ImageOptimisationPolicy,
                NotForDelivery = rawAsset.NotForDelivery
            };
        }, cacheSettings.GetMemoryCacheOptions(CacheDuration.Short));
    }
    
    private const string AssetSql = @"
SELECT ""Id"", ""Customer"", ""Space"", ""Created"", ""Origin"", ""Tags"", ""Roles"", 
""PreservedUri"", ""Reference1"", ""Reference2"", ""Reference3"", ""MaxUnauthorised"", 
""NumberReference1"", ""NumberReference2"", ""NumberReference3"", ""Width"", 
""Height"", ""Error"", ""Batch"", ""Finished"", ""Ingesting"", ""ImageOptimisationPolicy"", 
""ThumbnailPolicy"", ""Family"", ""MediaType"", ""Duration"", ""NotForDelivery""
  FROM public.""Images""
  WHERE ""Id""=@Id;";

    private const string ImageLocationSql =
        "SELECT \"Id\", \"S3\", \"Nas\" FROM public.\"ImageLocation\" WHERE \"Id\"=@Id;";
}

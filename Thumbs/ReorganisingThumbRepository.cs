﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DLCS.Core.Types;
using DLCS.Model.Assets;
using DLCS.Model.Assets.Thumbs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thumbs.Settings;

namespace Thumbs;

/// <summary>
/// <see cref="IThumbRepository"/> implementation that will conditionally attempt to reorganise existing thumbnails
/// </summary>
public class ReorganisingThumbRepository : IThumbRepository
{
    private readonly IThumbRepository wrappedThumbRepository;
    private readonly IOptionsMonitor<ThumbsSettings> settings;
    private readonly IThumbLayoutManager thumbLayoutManager;
    private readonly ILogger<ReorganisingThumbRepository> logger;

    public ReorganisingThumbRepository(
        IThumbRepository wrappedThumbRepository,
        ILogger<ReorganisingThumbRepository> logger, 
        IThumbLayoutManager thumbLayoutManager, 
        IOptionsMonitor<ThumbsSettings> settings)
    {
        this.wrappedThumbRepository = wrappedThumbRepository;
        this.logger = logger;
        this.thumbLayoutManager = thumbLayoutManager;
        this.settings = settings;
    }
        
    public async Task<List<int[]>?> GetOpenSizes(AssetId assetId)
    {
        var newLayoutResult = await EnsureNewLayout(assetId);
        if (newLayoutResult == ReorganiseResult.AssetNotFound)
        {
            logger.LogDebug("Requested asset not found for asset '{Asset}'", assetId);
            return null;
        }

        return await wrappedThumbRepository.GetOpenSizes(assetId);
    }
        
    private Task<ReorganiseResult> EnsureNewLayout(AssetId assetId)
    {
        var currentSettings = settings.CurrentValue;
        if (!currentSettings.EnsureNewThumbnailLayout)
        {
            return Task.FromResult(ReorganiseResult.Unknown);
        }

        return thumbLayoutManager.EnsureNewLayout(assetId);
    }
}
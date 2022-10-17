﻿using API.Features.Assets;
using API.Infrastructure.Requests;
using DLCS.Core;
using DLCS.Core.Settings;
using DLCS.Core.Strings;
using DLCS.Model.Assets;
using DLCS.Model.Policies;
using DLCS.Model.Storage;
using Microsoft.Extensions.Options;

namespace API.Features.Image.Ingest;

/// <summary>
/// Class that encapsulates logic for creating or updating assets.
/// The logic here is shared for when ingesting a single asset and ingesting a batch of assets.
/// </summary>
public class AssetProcessor
{
    private readonly IPolicyRepository policyRepository;
    private readonly IApiAssetRepository assetRepository;
    private readonly IStorageRepository storageRepository;
    private readonly DlcsSettings settings;
    
    public AssetProcessor(
        IApiAssetRepository assetRepository,
        IStorageRepository storageRepository,
        IPolicyRepository policyRepository,
        IOptions<DlcsSettings> dlcsSettings)
    {
        this.assetRepository = assetRepository;
        this.storageRepository = storageRepository;
        this.policyRepository = policyRepository;
        this.settings = dlcsSettings.Value;
    }

    /// <summary>
    /// Process an asset - including validation and handling Update or Insert logic and get ready for ingestion
    /// </summary>
    /// <param name="asset">Asset sent to API</param>
    /// <param name="mustExist">If true, then only Update operations are supported</param>
    /// <param name="alwaysReingest">If true, then engine will be notified</param>
    /// <param name="isBatchUpdate">
    /// If true, this operation is part of a batch save. Allows Batch property to be set
    /// </param>
    /// <param name="requiresReingestPreSave">Optional delegate for modifying asset prior to saving</param>
    /// <param name="cancellationToken">Current cancellation token</param>
    public async Task<ProcessAssetResult> Process(Asset asset, bool mustExist, bool alwaysReingest, bool isBatchUpdate,
        Func<Asset, Task>? requiresReingestPreSave = null, CancellationToken cancellationToken = default)
    {
        Asset? existingAsset;
        try
        {
            existingAsset = await assetRepository.GetAsset(asset.Id, noCache: true);
            if (existingAsset == null)
            {
                if (mustExist)
                {
                    return new ProcessAssetResult
                    {
                        Result = ModifyEntityResult<Asset>.Failure(
                            "Attempted to update an Asset that could not be found",
                            WriteResult.NotFound
                        )
                    };
                }

                var counts = await storageRepository.GetStorageMetrics(asset.Customer, cancellationToken);
                if (!counts.CanStoreAsset())
                {
                    return new ProcessAssetResult
                    {
                        Result = ModifyEntityResult<Asset>.Failure(
                            $"This operation will fall outside of your storage policy for number of images: maximum is {counts.MaximumNumberOfStoredImages}",
                            WriteResult.StorageLimitExceeded
                        )
                    };
                }

                counts.CustomerStorage.NumberOfStoredImages++;
            }

            var assetPreparationResult =
                AssetPreparer.PrepareAssetForUpsert(existingAsset, asset, false, isBatchUpdate);

            if (!assetPreparationResult.Success)
            {
                return new ProcessAssetResult
                {
                    Result = ModifyEntityResult<Asset>.Failure(assetPreparationResult.ErrorMessage,
                        WriteResult.FailedValidation)
                };
            }

            var updatedAsset = assetPreparationResult.UpdatedAsset!;
            var requiresEngineNotification =
                RequiresEngineNotification(updatedAsset, alwaysReingest, assetPreparationResult);

            if (existingAsset == null)
            {
                var imagePolicyChanged = await SelectImageOptimisationPolicy(updatedAsset);
                if (imagePolicyChanged)
                {
                    // NB the AssetPreparer has already inspected image policy, but this will pick up
                    // a change from default.
                    requiresEngineNotification = true;
                }

                var thumbnailPolicyChanged = await SelectThumbnailPolicy(updatedAsset);
                if (thumbnailPolicyChanged)
                {
                    // We won't alter the value of requiresEngineNotification
                    // TODO thumbs will be backfilled.
                    // This could be a config setting.
                }
            }

            if (requiresEngineNotification)
            {
                updatedAsset.SetFieldsForIngestion();

                if (requiresReingestPreSave != null)
                {
                    await requiresReingestPreSave(updatedAsset);
                }
            }
            else
            {
                updatedAsset.MarkAsFinished();
            }

            var assetAfterSave = await assetRepository.Save(updatedAsset, existingAsset != null, cancellationToken);

            // Restore fields that are not persisted but are required
            if (updatedAsset.InitialOrigin.HasText())
            {
                assetAfterSave.InitialOrigin = asset.InitialOrigin;
            }

            return new ProcessAssetResult
            {
                ExistingAsset = existingAsset,
                RequiresEngineNotification = requiresEngineNotification,
                Result = ModifyEntityResult<Asset>.Success(assetAfterSave,
                    existingAsset == null ? WriteResult.Created : WriteResult.Updated)
            };
        }
        catch (Exception e)
        {
            return new ProcessAssetResult
            {
                Result = ModifyEntityResult<Asset>.Failure(e.Message, WriteResult.Error)
            };
        }
    }

    private static bool RequiresEngineNotification(Asset asset, bool alwaysReingest,
        AssetPreparationResult assetPreparationResult)
    {
        // A 'File' never results in the engine being called
        if (asset.Family == AssetFamily.File) return false;
        
        return assetPreparationResult.RequiresReingest || alwaysReingest;
    }

    private async Task<bool> SelectThumbnailPolicy(Asset asset)
    {
        bool changed = false;
        if (asset.Family == AssetFamily.Image)
        {
            changed = await SetThumbnailPolicy(settings.IngestDefaults.ThumbnailPolicies.Graphics, asset);
        }
        else if (asset.Family == AssetFamily.Timebased && asset.MediaType.HasText() && asset.MediaType.Contains("video/"))
        {
            changed = await SetThumbnailPolicy(settings.IngestDefaults.ThumbnailPolicies.Video, asset);
        }

        return changed;
    }

    private async Task<bool> SelectImageOptimisationPolicy(Asset asset)
    {
        bool changed = false;
        if (asset.Family == AssetFamily.Image)
        {
            changed = await SetImagePolicy(settings.IngestDefaults.ImageOptimisationPolicies.Graphics, asset);
        }
        else if (asset.Family == AssetFamily.Timebased && asset.MediaType.HasText())
        {
            if (asset.MediaType.StartsWith("video/"))
            {
                changed = await SetImagePolicy(settings.IngestDefaults.ImageOptimisationPolicies.Video, asset);
            }
            else if (asset.MediaType.StartsWith("audio/"))
            {
                changed = await SetImagePolicy(settings.IngestDefaults.ImageOptimisationPolicies.Audio, asset);
            }
        }

        return changed;
    }

    private async Task<bool> SetImagePolicy(string key, Asset asset)
    {
        string? incomingPolicy = asset.ImageOptimisationPolicy;
        ImageOptimisationPolicy? policy = null;
        if (incomingPolicy.HasText())
        {
            policy = await policyRepository.GetImageOptimisationPolicy(incomingPolicy);
        }

        if (policy == null)
        {
            // The asset doesn't have a valid ImageOptimisationPolicy
            // This is adapted from Deliverator, but there wasn't a way of 
            // taking the policy from the incoming PUT. There now is.
            var imagePolicy = await policyRepository.GetImageOptimisationPolicy(key);
            if (imagePolicy != null)
            {
                asset.ImageOptimisationPolicy = imagePolicy.Id;
            }
        }

        return asset.ImageOptimisationPolicy != incomingPolicy;
    }
    
    private async Task<bool> SetThumbnailPolicy(string defaultPolicy, Asset asset)
    {
        string? incomingPolicy = asset.ThumbnailPolicy;
        ThumbnailPolicy? policy = null;
        if (incomingPolicy.HasText())
        {
            policy = await policyRepository.GetThumbnailPolicy(incomingPolicy);
        }

        if (policy == null)
        {
            var thumbnailPolicy = await policyRepository.GetThumbnailPolicy(defaultPolicy);
            if (thumbnailPolicy != null)
            {
                asset.ThumbnailPolicy = thumbnailPolicy.Id;
            }
        }

        return asset.ThumbnailPolicy != incomingPolicy;
    }
}

public class ProcessAssetResult
{
    public ModifyEntityResult<Asset> Result { get; set; }
    public Asset? ExistingAsset { get; set; }
    public bool RequiresEngineNotification { get; set; }

    public bool IsSuccess => Result.IsSuccess;
}
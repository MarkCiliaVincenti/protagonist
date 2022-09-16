using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using API.Exceptions;
using API.Features.Assets;
using API.Features.Image.Ingest;
using API.Infrastructure.Requests;
using DLCS.Core;
using DLCS.Core.Collections;
using DLCS.Model.Assets;
using DLCS.Model.Messaging;
using DLCS.Model.Spaces;
using MediatR;

namespace API.Features.Image.Requests;

/// <summary>
/// Handle create or update requests for assets.
/// This may trigger a sync or async ingest request depending on fields updated and HTTP method used. 
/// </summary>
/// <remarks>Handles PUTs and PATCHes with a single command to ensure same validation happens.</remarks>
public class CreateOrUpdateImage : IRequest<ModifyEntityResult<Asset>>
{
    /// <summary>
    /// The Asset to be updated or inserted; may contain null fields indicating no change
    /// </summary>
    public Asset? Asset { get; }

    /// <summary>
    /// Get a value indicating whether the asset must already exist in db (ie this only supports Update)
    /// </summary>
    public bool AssetMustExist { get; }
    
    /// <summary>
    /// Get a value indicating whether the asset will always be reingested, regardless of which fields are changed
    /// </summary>
    public bool AlwaysReingest { get; }

    public CreateOrUpdateImage(Asset asset, string httpMethod)
    {
        Asset = asset;
        AssetMustExist = httpMethod == "PATCH";
        
        // treat a PUT as a re-process instruction regardless of which values are changed
        AlwaysReingest = httpMethod == "PUT";
    }
}

public class CreateOrUpdateImageHandler : IRequestHandler<CreateOrUpdateImage, ModifyEntityResult<Asset>>
{
    private readonly ISpaceRepository spaceRepository;
    private readonly IApiAssetRepository assetRepository;
    private readonly IBatchRepository batchRepository;
    private readonly IAssetNotificationSender assetNotificationSender;
    private readonly AssetProcessor assetProcessor;

    public CreateOrUpdateImageHandler(
        ISpaceRepository spaceRepository,
        IApiAssetRepository assetRepository,
        IBatchRepository batchRepository,
        IAssetNotificationSender assetNotificationSender,
        AssetProcessor assetProcessor)
    {
        this.spaceRepository = spaceRepository;
        this.assetRepository = assetRepository;
        this.batchRepository = batchRepository;
        this.assetNotificationSender = assetNotificationSender;
        this.assetProcessor = assetProcessor;
    }
    
    public async Task<ModifyEntityResult<Asset>> Handle(CreateOrUpdateImage request, CancellationToken cancellationToken)
    {
        var asset = request.Asset;
        if (asset == null)
        {
            return ModifyEntityResult<Asset>.Failure("Invalid Request", WriteResult.FailedValidation);
        }

        if (!await DoesTargetSpaceExist(asset, cancellationToken))
        {
            return ModifyEntityResult<Asset>.Failure(
                $"Target space for asset does not exist: {asset.Customer}/{asset.Space}",
                WriteResult.FailedValidation);
        }

        var processAssetResult = await assetProcessor.Process(
            asset,
            request.AssetMustExist,
            request.AlwaysReingest,
            false,
            async updatedAsset =>
            {
                if (updatedAsset.Family == AssetFamily.Timebased)
                {
                    // Timebased asset - create a Batch record in DB and populate Batch property in Asset
                    await batchRepository.CreateBatch(updatedAsset.Customer, updatedAsset.AsList(), cancellationToken);
                }
            },
            cancellationToken
        );
        
        var modifyEntityResult = processAssetResult.Result;
        
        // Processing failed, return failure
        if (!modifyEntityResult.IsSuccess) return processAssetResult.Result;

        var changeType = processAssetResult.ExistingAsset == null ? ChangeType.Create : ChangeType.Update;
        var existingAsset = processAssetResult.ExistingAsset;
        var assetAfterSave = modifyEntityResult.Entity;
        
        await assetNotificationSender.SendAssetModifiedNotification(changeType, existingAsset, assetAfterSave);

        if (processAssetResult.RequiresEngineNotification)
        {
            return await IngestAndGenerateResult(assetAfterSave, existingAsset != null, cancellationToken);
        }

        return modifyEntityResult;
    }
    
    private async Task<bool> DoesTargetSpaceExist(Asset asset, CancellationToken cancellationToken)
    {
        var targetSpace = await spaceRepository.GetSpace(asset.Customer, asset.Space, false, cancellationToken);
        return targetSpace != null;
    }

    private async Task<ModifyEntityResult<Asset>> IngestAndGenerateResult(Asset asset, bool isUpdate,
        CancellationToken cancellationToken)
    {
        async Task<ModifyEntityResult<Asset>> GenerateFinalResult(bool success, string errorMessage,
            HttpStatusCode errorCode)
        {
            if (success)
            {
                // obtain it again after Engine has processed it
                var assetAfterEngine = await assetRepository.GetAsset(asset.Id, noCache: true);
                return ModifyEntityResult<Asset>.Success(assetAfterEngine,
                    isUpdate ? WriteResult.Updated : WriteResult.Created);
            }

            throw new APIException(errorMessage) { StatusCode = (int)errorCode };
        }

        switch (asset.Family)
        {
            case AssetFamily.Image:
            {
                // await call to engine, which processes synchronously (not a queue)
                var statusCode =
                    await assetNotificationSender.SendImmediateIngestAssetRequest(asset, false,
                        cancellationToken);
                var success = statusCode is HttpStatusCode.Created or HttpStatusCode.OK;

                return await GenerateFinalResult(success, "Engine was not able to process this asset", statusCode);
            }
            case AssetFamily.Timebased:
            {
                // Queue record for ingestion
                var success =
                    await assetNotificationSender.SendIngestAssetRequest(asset, cancellationToken);

                return await GenerateFinalResult(success, "Unable to queue for processing",
                    HttpStatusCode.InternalServerError);
            }
            default:
                throw new ArgumentOutOfRangeException($"No engine logic for asset family {asset.Family}");
        }
    }
}
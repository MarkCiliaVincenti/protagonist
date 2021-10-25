﻿using System.Threading;
using System.Threading.Tasks;
using DLCS.Core;
using DLCS.Core.Collections;
using DLCS.Model.Assets;
using DLCS.Web.Requests.AssetDelivery;
using DLCS.Web.Response;
using IIIF;
using IIIF.Presentation;
using IIIF.Presentation.V2.Strings;
using IIIF.Presentation.V3.Strings;
using IIIF.Serialisation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchestrator.Infrastructure.IIIF;
using Orchestrator.Infrastructure.Mediatr;
using Orchestrator.Models;
using Orchestrator.Settings;
using IIIF2 = IIIF.Presentation.V2;
using IIIF3 = IIIF.Presentation.V3;
using Version = IIIF.Presentation.Version;

namespace Orchestrator.Features.Manifests.Requests
{
    /// <summary>
    /// Mediatr request for generating basic single-item manifest for specified image
    /// </summary>
    public class GetManifestForAsset : IRequest<DescriptionResourceResponse>, IGenericAssetRequest
    {
        public string FullPath { get; }

        public BaseAssetRequest AssetRequest { get; set; }

        public Version IIIFPresentationVersion { get; }

        public GetManifestForAsset(string path, Version iiifVersion)
        {
            FullPath = path;
            IIIFPresentationVersion = iiifVersion;
        }
    }

    public class GetManifestForAssetHandler : IRequestHandler<GetManifestForAsset, DescriptionResourceResponse>
    {
        private readonly IAssetRepository assetRepository;
        private readonly IAssetPathGenerator assetPathGenerator;
        private readonly IIIFCanvasFactory canvasFactory;
        private readonly ILogger<GetManifestForAssetHandler> logger;
        private readonly OrchestratorSettings orchestratorSettings;

        public GetManifestForAssetHandler(
            IAssetRepository assetRepository,
            IAssetPathGenerator assetPathGenerator,
            IOptions<OrchestratorSettings> orchestratorSettings,
            IIIFCanvasFactory canvasFactory,
            ILogger<GetManifestForAssetHandler> logger)
        {
            this.assetRepository = assetRepository;
            this.assetPathGenerator = assetPathGenerator;
            this.canvasFactory = canvasFactory;
            this.orchestratorSettings = orchestratorSettings.Value;
            this.logger = logger;
        }

        public async Task<DescriptionResourceResponse> Handle(GetManifestForAsset request,
            CancellationToken cancellationToken)
        {
            var assetId = request.AssetRequest.GetAssetId();
            var asset = await assetRepository.GetAsset(assetId);
            if (asset is not { Family: AssetFamily.Image })
            {
                logger.LogDebug("Request iiif-manifest for asset {AssetId} but is not found or not an image", assetId);
                return DescriptionResourceResponse.Empty;
            }

            JsonLdBase manifest = request.IIIFPresentationVersion == Version.V3
                ? await GenerateV3Manifest(request.AssetRequest, asset)
                : await GenerateV2Manifest(request.AssetRequest, asset);

            return DescriptionResourceResponse.Open(manifest.AsJson());
        }

        private async Task<IIIF3.Manifest> GenerateV3Manifest(BaseAssetRequest assetRequest, Asset asset)
        {
            var fullyQualifiedImageId = GetFullyQualifiedId(assetRequest, orchestratorSettings.Proxy.ImagePath);
            var manifest = new IIIF3.Manifest
            {
                Id = fullyQualifiedImageId,
                Context = Context.Presentation3Context,
                Metadata = new LabelValuePair("en", "origin", asset.Origin).AsList(),
                Items = await canvasFactory.CreateV3Canvases(asset.AsList(), assetRequest.Customer)
            };

            return manifest;
        }

        private async Task<IIIF2.Manifest> GenerateV2Manifest(BaseAssetRequest assetRequest, Asset asset)
        {
            var fullyQualifiedImageId = GetFullyQualifiedId(assetRequest, orchestratorSettings.Proxy.ImagePath);
            var manifest = new IIIF2.Manifest
            {
                Id = fullyQualifiedImageId,
                Context = Context.Presentation2Context,
                Metadata = new IIIF2.Metadata
                    {
                        Label = new MetaDataValue("origin"),
                        Value = new MetaDataValue(asset.Origin)
                    }
                    .AsList(),
                Sequences = new IIIF2.Sequence
                {
                    Id = string.Concat(fullyQualifiedImageId, "/sequence/s0"),
                    Label = new MetaDataValue("Sequence 0"),
                    ViewingHint = "paged",
                    Canvases = await canvasFactory.CreateV2Canvases(asset.AsList(), assetRequest.Customer)
                }.AsList()
            };

            return manifest;
        }

        private string GetFullyQualifiedId(BaseAssetRequest baseAssetRequest, string prefix)
            => assetPathGenerator.GetFullPathForRequest(
                baseAssetRequest,
                (assetRequest, template) =>
                {
                    var request = assetRequest as BaseAssetRequest;
                    return DlcsPathHelpers.GeneratePathFromTemplate(
                        template,
                        prefix,
                        request.CustomerPathValue,
                        request.Space.ToString(),
                        request.AssetId);
                });

    }
}
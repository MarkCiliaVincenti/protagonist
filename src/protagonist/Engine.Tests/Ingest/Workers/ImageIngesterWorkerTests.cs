﻿using DLCS.Core.Types;
using DLCS.Model.Assets;
using DLCS.Model.Customers;
using DLCS.Model.Messaging;
using Engine.Ingest;
using Engine.Ingest.Image;
using Engine.Ingest.Image.Completion;
using Engine.Ingest.Persistence;
using Engine.Settings;
using Engine.Tests.Integration;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Test.Helpers.Settings;

namespace Engine.Tests.Ingest.Workers;

public class ImageIngesterWorkerTests
{
    private readonly IAssetToDisk assetToDisk;
    private readonly FakeImageProcessor imageProcessor;
    private readonly ImageIngesterWorker sut;
    private readonly EngineSettings engineSettings;
    private readonly IOrchestratorClient orchestratorClient;

    public ImageIngesterWorkerTests()
    {
        engineSettings = new EngineSettings
        {
            ImageIngest = new ImageIngestSettings
            {
                SourceTemplate = "{root}",
                OrchestrateImageAfterIngest = true
            },
        };
        var optionsMonitor = OptionsHelpers.GetOptionsMonitor(engineSettings);

        assetToDisk = A.Fake<IAssetToDisk>();
        imageProcessor = new FakeImageProcessor();
        orchestratorClient = A.Fake<IOrchestratorClient>();

        sut = new ImageIngesterWorker(assetToDisk, imageProcessor, orchestratorClient, new FakeFileSystem(),
            optionsMonitor, new NullLogger<ImageIngesterWorker>());
    }

    [Fact]
    public async Task Ingest_ReturnsFailed_IfCopyAssetError()
    {
        // Arrange
        var asset = new Asset(AssetId.FromString("2/1/shallow"));
        A.CallTo(() =>
                assetToDisk.CopyAssetToLocalDisk(A<Asset>._, A<string>._, true, A<CustomerOriginStrategy>._,
                    A<CancellationToken>._))
            .ThrowsAsync(new ArgumentNullException());

        // Act
        var result = await sut.Ingest(new IngestionContext(asset), new CustomerOriginStrategy());

        // Assert
        result.Should().Be(IngestResultStatus.Failed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ingest_SetsVerifySizeFlag_DependingOnCustomerOverride(bool noStoragePolicyCheck)
    {
        // Arrange
        const int customerId = 54;
        var asset = new Asset(AssetId.FromString($"{customerId}/1/shallow"));
        engineSettings.CustomerOverrides.Add(customerId.ToString(), new CustomerOverridesSettings
        {
            NoStoragePolicyCheck = noStoragePolicyCheck
        });
        var assetFromOrigin = new AssetFromOrigin(asset.Id, 13, "/target/location", "application/json");
        A.CallTo(() => assetToDisk.CopyAssetToLocalDisk(A<Asset>._, A<string>._, A<bool>._, A<CustomerOriginStrategy>._,
                A<CancellationToken>._))
            .Returns(assetFromOrigin);

        // Act
        await sut.Ingest(new IngestionContext(asset), new CustomerOriginStrategy());

        // Assert
        A.CallTo(() =>
                assetToDisk.CopyAssetToLocalDisk(A<Asset>._, A<string>._, !noStoragePolicyCheck, A<CustomerOriginStrategy>._,
                    A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Ingest_ReturnsStorageLimitExceeded_IfFileSizeTooLarge()
    {
        // Arrange
        var asset = new Asset(AssetId.FromString("/2/1/remurdered"));
        var assetFromOrigin = new AssetFromOrigin(asset.Id, 13, "/target/location", "application/json");
        assetFromOrigin.FileTooLarge();
        A.CallTo(() =>
                assetToDisk.CopyAssetToLocalDisk(A<Asset>._, A<string>._, true, A<CustomerOriginStrategy>._,
                    A<CancellationToken>._))
            .Returns(assetFromOrigin);

        // Act
        var result = await sut.Ingest(new IngestionContext(asset), new CustomerOriginStrategy());

        // Assert
        result.Should().Be(IngestResultStatus.StorageLimitExceeded);
    }
    
    [Theory]
    [InlineData(true, IngestResultStatus.Success)]
    [InlineData(false, IngestResultStatus.Failed)]
    public async Task Ingest_ReturnsCorrectResult_DependingOnIngestAndCompletion(bool imageProcessSuccess,
        IngestResultStatus expected)
    {
        // Arrange
        var asset = new Asset(AssetId.FromString("/2/1/remurdered"));

        A.CallTo(() =>
                assetToDisk.CopyAssetToLocalDisk(A<Asset>._, A<string>._, true, A<CustomerOriginStrategy>._,
                    A<CancellationToken>._))
            .Returns(new AssetFromOrigin(asset.Id, 13, "target", "application/json"));

        imageProcessor.ReturnValue = imageProcessSuccess;

        // Act
        var result = await sut.Ingest(new IngestionContext(asset), new CustomerOriginStrategy());

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task PostIngest_CallsOrchestrator_IfSuccessfulIngest()
    {
        // Arrange
        var assetId = AssetId.FromString("/2/1/faithless");
        var asset = new Asset(assetId);

        // Act
        await sut.PostIngest(new IngestionContext(asset), true);
        
        // Assert
        A.CallTo(() => orchestratorClient.TriggerOrchestration(assetId)).MustHaveHappened();
    }
    
    [Fact]
    public async Task PostIngest_DoesNotCallOrchestrator_IfIngestFailed()
    {
        // Arrange
        var assetId = AssetId.FromString("/2/1/faithless");
        var asset = new Asset(assetId);

        // Act
        await sut.PostIngest(new IngestionContext(asset), false);
        
        // Assert
        A.CallTo(() => orchestratorClient.TriggerOrchestration(assetId)).MustNotHaveHappened();
    }

    public class FakeImageProcessor : IImageProcessor
    {
        public bool WasCalled { get; private set; }

        public bool ReturnValue { get; set; }

        public Action<IngestionContext> Callback { get; set; }

        public Task<bool> ProcessImage(IngestionContext context)
        {
            WasCalled = true;

            Callback?.Invoke(context);

            return Task.FromResult(ReturnValue);
        }
    }
}
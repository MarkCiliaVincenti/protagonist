﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DLCS.Core.Types;
using DLCS.Model.Assets;
using DLCS.Model.PathElements;
using DLCS.Web.Requests.AssetDelivery;
using FakeItEasy;
using FluentAssertions;
using IIIF.ImageApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Orchestrator.Assets;
using Orchestrator.Features.Images;
using Orchestrator.Infrastructure;
using Orchestrator.Infrastructure.ReverseProxy;
using Orchestrator.Settings;
using Xunit;

namespace Orchestrator.Tests.Images
{
    public class ImageRequestHandlerTests
    {
        private readonly IAssetTracker assetTracker;
        private readonly IAssetDeliveryPathParser assetDeliveryPathParser;
        private readonly IPathCustomerRepository customerRepository;
        private readonly AssetDeliveryPathParser assetDeliveryPathParserImpl;
        private readonly IOptions<OrchestratorSettings> defaultSettings;

        public ImageRequestHandlerTests()
        {
            assetTracker = A.Fake<IAssetTracker>();
            assetDeliveryPathParser = A.Fake<IAssetDeliveryPathParser>();
            customerRepository = A.Fake<IPathCustomerRepository>();
            assetDeliveryPathParserImpl = new AssetDeliveryPathParser(customerRepository);
            defaultSettings = Options.Create(new OrchestratorSettings());
        }

        [Fact]
        public async Task HandleRequest_Returns404_IfAssetPathParserThrowsKeyNotFound()
        {
            // Arrange
            A.CallTo(() => assetDeliveryPathParser.Parse<ImageAssetDeliveryRequest>(A<string>._))
                .ThrowsAsync(new KeyNotFoundException());
            var sut = GetImageRequestHandlerWithMockPathParser(true);
            
            // Act
            var result = await sut.HandleRequest(new DefaultHttpContext());
            
            // Assert
            result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        
        [Fact]
        public async Task HandleRequest_Returns400_IfAssetPathParserThrowsFormatException()
        {
            // NOTE - routes should prevent this from ever happening
            
            // Arrange
            A.CallTo(() => assetDeliveryPathParser.Parse<ImageAssetDeliveryRequest>(A<string>._))
                .ThrowsAsync(new FormatException());
            var sut = GetImageRequestHandlerWithMockPathParser(true);
            
            // Act
            var result = await sut.HandleRequest(new DefaultHttpContext());
            
            // Assert
            result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task HandleRequest_Returns400_IfAssetPathParserThrowsException()
        {
            // NOTE - routes should prevent this from ever happening
            
            // Arrange
            A.CallTo(() => assetDeliveryPathParser.Parse<ImageAssetDeliveryRequest>(A<string>._))
                .ThrowsAsync(new ApplicationException());
            var sut = GetImageRequestHandlerWithMockPathParser(true);
            
            // Act
            var result = await sut.HandleRequest(new DefaultHttpContext());
            
            // Assert
            result.Should().BeOfType<StatusCodeResult>()
                .Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Handle_Request_ProxiesToOrchestrator_IfAssetRequiresAuth()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/iiif-img/2/2/test-image/full/!200,200/0/default.jpg";

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            A.CallTo(() => assetTracker.GetOrchestrationAsset(new AssetId(2, 2, "test-image")))
                .Returns(new OrchestrationImage { Roles = new List<string> { "role" } });
            var sut = GetImageRequestHandlerWithMockPathParser();

            // Act
            var result = await sut.HandleRequest(context) as ProxyActionResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.Orchestrator);
            result.HasPath.Should().BeTrue();
        }
        
        [Fact]
        public async Task Handle_Request_ProxiesToThumbs_OnNewPath_IfAssetIsForUvThumb()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/iiif-img/2/2/test-image/full/90,/0/default.jpg";
            context.Request.QueryString = new QueryString("?t=123123");

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            A.CallTo(() => assetTracker.GetOrchestrationAsset(new AssetId(2, 2, "test-image")))
                .Returns(new OrchestrationImage {AssetId = new AssetId(2, 2, "test-image")});
            var sut = GetImageRequestHandlerWithMockPathParser(
                settings: Options.Create(new OrchestratorSettings
                {
                    ImageFolderTemplateImageServer = "/path",
                    Proxy = new ProxySettings()
                }));

            // Act
            var result = await sut.HandleRequest(context) as ProxyActionResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.Thumbs);
            result.HasPath.Should().BeTrue();
            result.Path.Should().Be("thumbs/2/2/test-image/full/!200,200/0/default.jpg");
        }
        
        [Fact]
        public async Task Handle_Request_ProxiesToThumbs_OnNewPathFromSettings_IfAssetIsForUvThumb()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/iiif-img/2/2/test-image/full/90,/0/default.jpg";
            context.Request.QueryString = new QueryString("?t=123123");

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            A.CallTo(() => assetTracker.GetOrchestrationAsset(new AssetId(2, 2, "test-image")))
                .Returns(new OrchestrationImage {AssetId = new AssetId(2, 2, "test-image")});
            var sut = GetImageRequestHandlerWithMockPathParser(
                settings: Options.Create(new OrchestratorSettings
                {
                    Proxy = new ProxySettings { UVThumbReplacementPath = "!300,500" }
                }));
            
            // Act
            var result = await sut.HandleRequest(context) as ProxyActionResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.Thumbs);
            result.HasPath.Should().BeTrue();
            result.Path.Should().Be("thumbs/2/2/test-image/full/!300,500/0/default.jpg");
        }
        
        [Fact]
        public async Task Handle_Request_ProxiesToImageServer_IfAssetIsForUvThumb_UvThumbDisabled()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/iiif-img/2/2/test-image/full/90,/0/default.jpg";
            context.Request.QueryString = new QueryString("?t=123123");

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            A.CallTo(() => assetTracker.GetOrchestrationAsset(new AssetId(2, 2, "test-image")))
                .Returns(new OrchestrationImage {AssetId = new AssetId(2, 2, "test-image")});
            var sut = GetImageRequestHandlerWithMockPathParser(
                settings: Options.Create(new OrchestratorSettings
                {
                    ImageFolderTemplateImageServer = "/path",
                    Proxy = new ProxySettings { CheckUVThumbs = false }
                }));

            // Act
            var result = await sut.HandleRequest(context) as ProxyImageServerResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.ImageServer);
            result.HasPath.Should().BeTrue();
        }
        
        [Fact]
        public async Task Handle_Request_ProxiesToThumbs_IfFullOrMaxRegion_AndKnownSize()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = "/iiif-img/2/2/test-image/full/!100,150/0/default.jpg";

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            var assetId = new AssetId(2, 2, "test-image");
            A.CallTo(() => assetTracker.GetOrchestrationAsset(assetId))
                .Returns(new OrchestrationImage
                    { AssetId = assetId, OpenThumbs = new List<int[]> { new[] { 150, 150 } } });
            var sut = GetImageRequestHandlerWithMockPathParser(
                settings: Options.Create(new OrchestratorSettings
                {
                    ImageFolderTemplateImageServer = "/path",
                    Proxy = new ProxySettings()
                }));

            // Act
            var result = await sut.HandleRequest(context) as ProxyActionResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.Thumbs);
            result.Path.Should().Be("thumbs/2/2/test-image/full/!100,150/0/default.jpg");
        }
        
        [Theory]
        [InlineData("/iiif-img/2/2/test-image/full/90,/0/default.jpg", false)] // UV without ?t=
        [InlineData("/iiif-img/2/2/test-image/full/full/0/default.jpg", true)] // /full/full
        [InlineData("/iiif-img/2/2/test-image/full/max/0/default.jpg", true)] // /full/max
        public async Task Handle_Request_ProxiesToImageServer_ForAllOtherCases(string path, bool knownThumb)
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            A.CallTo(() => customerRepository.GetCustomer("2")).Returns(new CustomerPathElement(2, "Test-Cust"));
            var assetId = new AssetId(2, 2, "test-image");
            
            var sut = GetImageRequestHandlerWithMockPathParser(
                settings: Options.Create(new OrchestratorSettings
                {
                    ImageFolderTemplateImageServer = "/path",
                    Proxy = new ProxySettings()
                }));

            List<int[]> openSizes = knownThumb
                ? new List<int[]> { new[] { 150, 150 } }
                : new List<int[]>();

            A.CallTo(() => assetTracker.GetOrchestrationAsset(assetId))
                .Returns(new OrchestrationImage { AssetId = assetId, OpenThumbs = openSizes });

            // Act
            var result = await sut.HandleRequest(context) as ProxyImageServerResult;
            
            // Assert
            result.Target.Should().Be(ProxyDestination.ImageServer);
            result.HasPath.Should().BeTrue();
        }

        private ImageRequestHandler GetImageRequestHandlerWithMockPathParser(bool mockPathParser = false,
            IOptions<OrchestratorSettings> settings = null)
        {
            var requestProcessor = new AssetRequestProcessor(new NullLogger<AssetRequestProcessor>(), assetTracker,
                mockPathParser ? assetDeliveryPathParser : assetDeliveryPathParserImpl);
            return new(new NullLogger<ImageRequestHandler>(), requestProcessor, settings ?? defaultSettings);
        }
    }
}
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using API.Features.Assets;
using API.Tests.Integration.Infrastructure;
using DLCS.Core.Caching;
using DLCS.Model.Assets;
using DLCS.Repository;
using DLCS.Repository.Assets;
using LazyCache.Mocks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Test.Helpers.Integration;

namespace API.Tests.Features.Assets;

[Trait("Category", "Database")]
[Collection(CollectionDefinitions.DatabaseCollection.CollectionName)]
public class ApiAssetRepositoryTests
{
    private readonly DlcsContext contextForTests;
    private readonly DlcsContext dbContext;
    private readonly ApiAssetRepository sut;

    public ApiAssetRepositoryTests(DlcsDatabaseFixture dbFixture)
    {
        // Store non-tracking dbContext for adding items to backing store + verifying results
        contextForTests = dbFixture.DbContext;
        
        // We use a customised dbcontext for SUT because we want different tracking behaviour
        dbContext = new DlcsContext(
            new DbContextOptionsBuilder<DlcsContext>()
                .UseNpgsql(dbFixture.ConnectionString).Options
        );
        // We want this turned on to match live behaviour
        dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
                { new KeyValuePair<string, string>("ConnectionStrings:PostgreSQLConnection", dbFixture.ConnectionString) })
            .Build();
        var dapperAssetRepository = new DapperAssetRepository(
            config,
            new MockCachingService(),
            Options.Create(new CacheSettings()),
            new NullLogger<DapperAssetRepository>()
        );

        sut = new ApiAssetRepository(dbContext, dapperAssetRepository);

        dbFixture.CleanUp();
    }

    [Fact]
    public async Task AssetRepository_Saves_New_Asset()
    {
        var id = "new-asset";
        var newAsset = new Asset { Id = id, Customer = 100, Space = 10, Reference1 = "I am new", 
            Origin = "https://example.org/image1.tiff"};
    
        var result = AssetPreparer.PrepareAssetForUpsert(null, newAsset, false, false);
        result.Success.Should().BeTrue();

        await sut.Save(newAsset, false, CancellationToken.None);

        var dbAsset = await dbContext.Images.FindAsync(id);
        dbAsset.Reference1.Should().Be("I am new");
        dbAsset.Reference2.Should().Be("");
        dbAsset.MediaType.Should().Be("unknown");
    }
    
    [Fact]
    public async Task AssetRepository_Saves_New_Asset_UsingResultFromPreparer()
    {
        var id = nameof(AssetRepository_Saves_New_Asset_UsingResultFromPreparer);
        var newAsset = new Asset { Id = id, Customer = 100, Space = 10, Reference1 = "I am new", 
            Origin = "https://example.org/image1.tiff"};
    
        var result = AssetPreparer.PrepareAssetForUpsert(null, newAsset, false, false);
        result.Success.Should().BeTrue();

        await sut.Save(result.UpdatedAsset!, false, CancellationToken.None);

        var dbAsset = await dbContext.Images.FindAsync(id);
        dbAsset.Reference1.Should().Be("I am new");
        dbAsset.Reference2.Should().Be("");
        dbAsset.MediaType.Should().Be("unknown");
    }
    
    [Fact]
    public async Task AssetRepository_Saves_Existing_Asset()
    {
        // Arrange
        const string id = nameof(AssetRepository_Saves_Existing_Asset);
        var dbAsset = await contextForTests.Images.AddTestAsset(id, ref1: "I am original 1", ref2: "I am original 2");
        await contextForTests.SaveChangesAsync();

        var existingAsset = await dbContext.Images.FirstAsync(a => a.Id == id);
        var patch = new Asset
        {
            Id = id,
            Reference1 = "I am changed",
            Customer = 99,
            Space = 1
        };
        
        var result = AssetPreparer.PrepareAssetForUpsert(existingAsset, patch, false, false);
        result.Success.Should().BeTrue();
    
        // Act
        await sut.Save(existingAsset, true, CancellationToken.None);

        contextForTests.Entry(dbAsset.Entity).Reload();
        dbAsset.Entity.Reference1.Should().Be("I am changed");
        dbAsset.Entity.Reference2.Should().Be("I am original 2");
    }
    
    [Fact]
    public async Task AssetRepository_Saves_Existing_Asset_UsingResultFromPreparer()
    {
        // Arrange
        const string id = nameof(AssetRepository_Saves_Existing_Asset_UsingResultFromPreparer);
        var dbAsset = await contextForTests.Images.AddTestAsset(id, ref1: "I am original 1", ref2: "I am original 2");
        await contextForTests.SaveChangesAsync();

        var existingAsset = await dbContext.Images.FirstAsync(a => a.Id == id);
        var patch = new Asset
        {
            Id = id,
            Reference1 = "I am changed",
            Customer = 99,
            Space = 1
        };
        
        var result = AssetPreparer.PrepareAssetForUpsert(existingAsset, patch, false, false);
        result.Success.Should().BeTrue();
    
        // Act
        await sut.Save(result.UpdatedAsset, true, CancellationToken.None);

        contextForTests.Entry(dbAsset.Entity).Reload();
        dbAsset.Entity.Reference1.Should().Be("I am changed");
        dbAsset.Entity.Reference2.Should().Be("I am original 2");
    }
}
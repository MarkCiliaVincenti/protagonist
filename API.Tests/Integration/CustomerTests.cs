using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using API.Client;
using API.Tests.Integration.Infrastructure;
using DLCS.Core.Strings;
using DLCS.HydraModel;
using DLCS.Repository;
using FluentAssertions;
using Hydra.Collections;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Test.Helpers.Integration;
using Test.Helpers.Integration.Infrastructure;
using Xunit;

namespace API.Tests.Integration;

[Trait("Category", "Integration")]
[Collection(CollectionDefinitions.DatabaseCollection.CollectionName)]
public class CustomerTests : IClassFixture<ProtagonistAppFactory<Startup>>
{
    private readonly DlcsContext dbContext;
    private readonly HttpClient httpClient;
    
    public CustomerTests(DlcsDatabaseFixture dbFixture, ProtagonistAppFactory<Startup> factory)
    {
        dbContext = dbFixture.DbContext;
        httpClient = factory.ConfigureBasicAuthedIntegrationTestHttpClient(dbFixture, "API-Test");
        dbFixture.CleanUp();
    }
    
    
    
    [Fact]
    public async Task GetCustomers_Returns_HydraCollection()
    {
        var response = await httpClient.AsCustomer().GetAsync("/customers");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var coll = await response.ReadAsHydraResponseAsync<HydraCollection<JObject>>();
        coll.Should().NotBeNull();
        coll.Type.Should().Be("Collection");
        coll.Members.Should().NotBeEmpty();
        coll.Members.Should().Contain(jo => jo["@id"].Value<string>().EndsWith("customers/99"));
    }
    
    
    [Fact]
    public async Task GetCustomer_Returns_Customer()
    {
        var response = await httpClient.AsCustomer().GetAsync("/customers/99");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cust = await response.ReadAsHydraResponseAsync<Customer>();
        cust.Type.Should().Be("vocab:Customer");
        cust.Id.Should().EndWith("/customers/99");
    }
    
    
    
    [Fact]
    public async Task GetOtherCustomer_Returns_NotFound()
    {
        var response = await httpClient.AsAdmin().GetAsync("/customers/100");
        
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Customer_Test()
    {
        // arrange
        // Need to create an entity counter global for customers
        int expectedNewCustomerId = 1;
        
        var customerCounter = await dbContext.EntityCounters.SingleOrDefaultAsync(ec 
            => ec.Customer == 0 && ec.Scope == "0" && ec.Type == "customer");
        customerCounter.Should().BeNull();
        // this is true atm but Seed data might change this.
        // The counter should be created on first use, see below
        
        
        const string newCustomerJson = @"{
  ""@type"": ""Customer"",
  ""name"": ""my-new-customer"",
  ""displayName"": ""My New Customer""
}";
        var content = new StringContent(newCustomerJson, Encoding.UTF8, "application/json");
        // act
        var response = await httpClient.AsAdmin().PostAsync("/customers", content);
        
        // assert
        // Need to demonstrate that
        // - this does what we think it should do
        // - it does what Deliverator does.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var newCustomer = await response.ReadAsHydraResponseAsync<Customer>();
        
        
        // The entity counter should allocate the next available ID.
        // The DB has 1 customer 99 initially... is that going to change things?
        newCustomer.Id.Should().EndWith("customers/" + expectedNewCustomerId);

        var newDbCustomer = await dbContext.Customers.SingleOrDefaultAsync(c => c.Id == expectedNewCustomerId);
        newDbCustomer.Should().NotBeNull();
        newDbCustomer.Name.Should().Be("my-new-customer");
        newDbCustomer.DisplayName.Should().Be("My New Customer");
        newDbCustomer.AcceptedAgreement.Should().BeTrue();
        newDbCustomer.Administrator.Should().BeFalse();
        // what else to check?
        // Customer Does NOT have keys
        
        // The global customer entity counter should be incremented
        customerCounter = await dbContext.EntityCounters.SingleAsync(ec 
            => ec.Customer == 0 && ec.Scope == "0" && ec.Type == "customer");
        
        customerCounter.Should().NotBeNull(); // it would have been created on demand
        customerCounter.Next.Should().Be(expectedNewCustomerId + 1);
        
        // There should be a space entity counter for the new customer
        var spaceCounter = await dbContext.EntityCounters.SingleOrDefaultAsync(ec 
            => ec.Customer == expectedNewCustomerId && ec.Scope == expectedNewCustomerId.ToString() && ec.Type == "space");
        spaceCounter.Should().NotBeNull();
        spaceCounter.Next.Should().Be(1);

        // There are two new row in Auth Services table (this example for cust 21)
        // | Id | Customer | Name | Profile | Label | Description | PageLabel | PageDescription | CallToAction | TTL | RoleProvider | ChildAuthService |
        // | 79b20327-5d55-4a7f-b95d-02cb94737bcd | 21 | clickthrough |  |  |  |  |  |  | 600 | NULL | d87d1619-e357-49c5-a45e-757407af7638 |
        // | d87d1619-e357-49c5-a45e-757407af7638 | 21 | logout | http://iiif.io/api/auth/0/logout |  |  |  |  |  | 600 | NULL | NULL |

        var customerAuthServices = await dbContext.AuthServices.Where(svc 
            => svc.Customer == expectedNewCustomerId).ToListAsync();
        customerAuthServices.Should().HaveCount(2); // two new services

        var clickthroughService = customerAuthServices.SingleOrDefault(svc => svc.Name == "clickthrough");
        var logoutService = customerAuthServices.SingleOrDefault(svc => svc.Name == "logout");
        clickthroughService.Should().NotBeNull();
        logoutService.Should().NotBeNull();
        clickthroughService.Id.Should().NotBeEmpty();
        logoutService.Id.Should().NotBeEmpty();
        clickthroughService.ChildAuthService.Should().Be(logoutService.Id);
        logoutService.Profile.Should().Be("http://iiif.io/api/auth/1/logout");
        clickthroughService.Ttl.Should().Be(600);
        logoutService.Ttl.Should().Be(600);
        
        // Role: Should be a clickthrough Role for AuthService
        var roles = await dbContext.Roles.Where(role 
            => role.Customer == expectedNewCustomerId).ToListAsync();
        roles.Should().HaveCount(1); // one new role
        var clickthroughRole = roles.SingleOrDefault(role => role.Name == "clickthrough");
        clickthroughRole.Should().NotBeNull();
        clickthroughRole.AuthService.Should().Be(clickthroughService.Id);
        
        // What should this URL be? api.dlcs.io is... not right?
        var roleId = $"https://api.dlcs.io/customers/{expectedNewCustomerId}/roles/clickthrough";
        clickthroughRole.Id.Should().Be(roleId);
        
        // Deliverator does not create a row in CustomerStorage at this point
        
        // Should be a row in Queues
        var queue = await dbContext.Queues.SingleOrDefaultAsync(q => q.Customer == expectedNewCustomerId);
        queue.Should().NotBeNull();
        queue.Name.Should().Be("default");
        queue.Size.Should().Be(0);

    }
    
    [Fact]
    public async void CreateNewCustomer_Throws_IfNameConflicts()
    {
        // Tests CreateCustomer::EnsureCustomerNamesNotTaken
        // These display names have already been taken by the seed data
        const string newCustomerJson = @"{
  ""@type"": ""Customer"",
  ""name"": ""test"",
  ""displayName"": ""TestUser""
}";
        
        // act
        var content = new StringContent(newCustomerJson, Encoding.UTF8, "application/json");
        var response = await httpClient.AsAdmin().PostAsync("/customers", content);
        
        // assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async void Api_Grants_Key_And_Secret()
    {
        var response = await httpClient.AsCustomer(99).PostAsync("/customers/99/keys", new StringContent(String.Empty));
        var key = await response.ReadAsHydraResponseAsync<ApiKey>();
        key.Key.Should().NotBeEmpty();
        key.Secret.Should().NotBeEmpty();
    }

    
    
    [Fact]
    public async void Api_Yields_Keys()
    {
        // arrange
        var response1 = await httpClient.AsCustomer(99).PostAsync("/customers/99/keys", new StringContent(String.Empty));
        var key1 = await response1.ReadAsHydraResponseAsync<ApiKey>();
        var response2 = await httpClient.AsCustomer(99).PostAsync("/customers/99/keys", new StringContent(String.Empty));
        var key2 = await response2.ReadAsHydraResponseAsync<ApiKey>();
        
        // act
        var response = await httpClient.AsCustomer(99).GetAsync("/customers/99/keys");
        var keys = await response.ReadAsHydraResponseAsync<HydraCollection<ApiKey>>();
        
        // assert
        keys.Members.Should().Contain(k => k.Key == key1.Key);
        keys.Members.Should().Contain(k => k.Key == key2.Key);
        keys.Members.Should().NotContain(k => k.Secret.HasText());
    }

}
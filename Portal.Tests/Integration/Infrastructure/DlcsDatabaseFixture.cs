﻿using System;
using System.Threading.Tasks;
using DLCS.Repository;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Configurations.Databases;
using DotNet.Testcontainers.Containers.Modules.Databases;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Portal.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Xunit fixture that manages lifecycle for Postgres 12 container with basic migration applied.
    /// </summary>
    public class DlcsDatabaseFixture : IAsyncLifetime
    {
        private readonly PostgreSqlTestcontainer postgresContainer;
        
        public DlcsContext DbContext { get; }
        public string ConnectionString { get; }
        
        public DlcsDatabaseFixture()
        {
            var postgresBuilder = new TestcontainersBuilder<PostgreSqlTestcontainer>()
                .WithDatabase(new PostgreSqlTestcontainerConfiguration("postgres:12-alpine")
                {
                    Database = "db",
                    Password = "postgres_pword",
                    Username = "postgres"
                })
                .WithCleanUp(true)
                .WithLabel("protagonist_test", "True");

            postgresContainer = postgresBuilder.Build();
            ConnectionString = postgresContainer.ConnectionString;

            // Create new DlcsContext using connection string for Postgres container
            DbContext = new DlcsContext(
                new DbContextOptionsBuilder<DlcsContext>()
                    .UseNpgsql(postgresContainer.ConnectionString).Options
            );
            DbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }
        
        /// <summary>
        /// Delete any standing data
        /// </summary>
        public void CleanUp()
            => DbContext.Database.ExecuteSqlRaw(@"
                DELETE FROM ""Spaces""");
        
        public async Task InitializeAsync()
        {
            // Start DB + apply migrations
            try
            {
                await postgresContainer.StartAsync();
                await DbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                var m = ex.Message;
                throw;
            }
        }

        public Task DisposeAsync() => postgresContainer.StopAsync();
    }

    [CollectionDefinition(CollectionName)]
    public class DatabaseCollection : ICollectionFixture<DlcsDatabaseFixture>
    {
        public const string CollectionName = "Database Collection";
    }
}
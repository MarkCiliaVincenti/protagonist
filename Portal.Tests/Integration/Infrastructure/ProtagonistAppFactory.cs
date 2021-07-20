﻿using System.Collections.Generic;
using System.IO;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Portal.Tests.Integration.Infrastructure
{
    /// <summary>
    /// Basic appFactory for protagonist, configuring <see cref="TestAuthHandler"/> for auth and LocalStack for aws
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    public class ProtagonistAppFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup: class
    {
        private readonly Dictionary<string, string> configuration = new();
        private LocalStackFixture localStack;

        /// <summary>
        /// Specify connection string to use for dlcsContext when building services
        /// </summary>
        /// <param name="connectionString">connection string to use for dbContext - docker instance</param>
        /// <returns>Current instance</returns>
        public ProtagonistAppFactory<TStartup> WithConnectionString(string connectionString)
        {
            configuration["ConnectionStrings:PostgreSQLConnection"] = connectionString;
            return this;
        }
        
        /// <summary>
        /// <see cref="LocalStackFixture"/> to use for replacing AWS services.
        /// </summary>
        /// <param name="fixture"><see cref="LocalStackFixture"/> to use.</param>
        /// <returns>Current instance</returns>
        public ProtagonistAppFactory<TStartup> WithLocalStack(LocalStackFixture fixture)
        {
            localStack = fixture;
            return this;
        }
        
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Testing.json");

            builder
                .ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile(configPath);
                    conf.AddInMemoryCollection(configuration);
                })
                .ConfigureTestServices(services =>
                {
                    ReplaceAuthentication(services);

                    if (localStack != null)
                    {
                        ConfigureS3Services(services);
                    }
                })
                .UseEnvironment("Testing");  // NOTE: This can cause issues if AddSystemsManager is used and not optional
        }

        private static void ReplaceAuthentication(IServiceCollection services)
        {
            // TODO - is this portal-specific?
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    "Test", _ => { });
        }

        private void ConfigureS3Services(IServiceCollection services)
        {
            services.Remove(new ServiceDescriptor(typeof(IAmazonS3),
                a => a.GetService(typeof(IAmazonS3)), ServiceLifetime.Singleton));
            
            services.AddSingleton<IAmazonS3>(p => localStack.AmazonS3);
        }
    }
}
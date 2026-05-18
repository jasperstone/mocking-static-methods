using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Xunit;

namespace Orleans.Persistence.DynamoDB.Tests.Hosting
{
    public class DynamoDBGrainStorageServiceCollectionExtensionsTests
    {
        private const string DefaultStorageProviderName = "Default";

        [Fact]
        public void AddDynamoDBGrainStorage_WithOptionsBuilder_RegistersExpectedServices()
        {
            var services = new ServiceCollection();

            // Register IOptionsMonitor<DynamoDBStorageOptions> to avoid resolution errors
            services.AddOptions<DynamoDBStorageOptions>("TestStorage");

            // Call the method under test with fully qualified method name to disambiguate overload
            var returnedServices = Orleans.Hosting.DynamoDBGrainStorageServiceCollectionExtensions
                .AddDynamoDBGrainStorage(services, "TestStorage", (Action<OptionsBuilder<DynamoDBStorageOptions>>)(optionsBuilder => { /* no-op */ }));

            Assert.Same(services, returnedServices);

            var serviceProvider = services.BuildServiceProvider();

            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            var postConfigure = serviceProvider.GetService<IPostConfigureOptions<DynamoDBStorageOptions>>();
            Assert.NotNull(postConfigure);

            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(optionsMonitor);

            var options = optionsMonitor.Get("TestStorage");
            Assert.NotNull(options);
        }

        [Fact]
        public void AddDynamoDBGrainStorageAsDefault_WithOptionsBuilder_UsesDefaultStorageProviderName()
        {
            var services = new ServiceCollection();

            // Register IOptionsMonitor<DynamoDBStorageOptions> to avoid resolution errors
            services.AddOptions<DynamoDBStorageOptions>(DefaultStorageProviderName);

            var returnedServices = Orleans.Hosting.DynamoDBGrainStorageServiceCollectionExtensions
                .AddDynamoDBGrainStorageAsDefault(services, (Action<OptionsBuilder<DynamoDBStorageOptions>>)(optionsBuilder => { /* no-op */ }));

            Assert.Same(services, returnedServices);

            var serviceProvider = services.BuildServiceProvider();

            var validator = serviceProvider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);

            var optionsMonitor = serviceProvider.GetService<IOptionsMonitor<DynamoDBStorageOptions>>();
            Assert.NotNull(optionsMonitor);

            var options = optionsMonitor.Get(DefaultStorageProviderName);
            Assert.NotNull(options);
        }
    }
}

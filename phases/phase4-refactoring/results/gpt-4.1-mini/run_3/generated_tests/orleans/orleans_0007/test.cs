using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        private const string TestStorageName = "TestStorage";

        [Fact]
        public void AddAdoNetGrainStorage_WithNameAndConfigureOptions_RegistersServices()
        {
            var services = new ServiceCollection();

            // Add a mock IGrainStorageSerializer to satisfy DefaultStorageProviderSerializerOptionsConfigurator dependency
            var mockSerializer = new Mock<IGrainStorageSerializer>();
            services.AddSingleton(mockSerializer.Object);

            var returnedServices = services.AddAdoNetGrainStorage(TestStorageName, ob => ob.Configure(options =>
            {
                // We cannot set properties on AdoNetGrainStorageOptions because we don't have the type here,
                // but we can test that the configuration delegate is invoked without error.
            }));

            Assert.Same(services, returnedServices);

            var provider = services.BuildServiceProvider();

            // We can resolve IConfigurationValidator
            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_RegistersServices_WithMockedOptionsMonitor()
        {
            var services = new ServiceCollection();

            // Add a mock IGrainStorageSerializer to satisfy DefaultStorageProviderSerializerOptionsConfigurator dependency
            var mockSerializer = new Mock<IGrainStorageSerializer>();
            services.AddSingleton(mockSerializer.Object);

            // Add a mock IOptionsMonitor<AdoNetGrainStorageOptions> to satisfy the validator factory
            var mockOptionsMonitor = new Mock<IOptionsMonitor<Orleans.Configuration.AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(It.IsAny<string>())).Returns(new Orleans.Configuration.AdoNetGrainStorageOptions());
            services.AddSingleton(mockOptionsMonitor.Object);

            var returnedServices = services.AddAdoNetGrainStorageAsDefault();

            Assert.Same(services, returnedServices);

            var provider = services.BuildServiceProvider();

            var validator = provider.GetService<IConfigurationValidator>();
            Assert.NotNull(validator);
        }
    }
}

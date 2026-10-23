using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AdoNetGrainStorageServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAdoNetGrainStorage_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new AdoNetGrainStorageOptions();
            Action<OptionsBuilder<AdoNetGrainStorageOptions>> configureOptions = ob => ob.Configure(o => o.ConnectionString = "TestConnectionString");

            var mockSerializer = new Mock<IGrainStorageSerializer>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get("TestName")).Returns(options);

            var mockPostConfigureOptions = new Mock<IPostConfigureOptions<AdoNetGrainStorageOptions>>();
            mockPostConfigureOptions.Setup(m => m.PostConfigure("TestName", options)).Verifiable();

            services.AddSingleton(mockOptionsMonitor.Object);
            services.AddSingleton(mockSerializer.Object);
            services.AddSingleton(mockPostConfigureOptions.Object);

            // Act
            services.AddAdoNetGrainStorage("TestName", configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var configuredOptions = optionsMonitor.Get("TestName");

            Assert.NotNull(configuredOptions);
            Assert.Equal("TestConnectionString", configuredOptions.ConnectionString);

            var postConfigureOptions = serviceProvider.GetServices<IPostConfigureOptions<AdoNetGrainStorageOptions>>();
            Assert.NotNull(postConfigureOptions);
            Assert.Equal(2, postConfigureOptions.Count());

            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);

            mockPostConfigureOptions.Verify();
        }

        [Fact]
        public void AddAdoNetGrainStorageAsDefault_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new AdoNetGrainStorageOptions();
            Action<OptionsBuilder<AdoNetGrainStorageOptions>> configureOptions = ob => ob.Configure(o => o.ConnectionString = "TestConnectionString");

            var mockSerializer = new Mock<IGrainStorageSerializer>();
            var mockOptionsMonitor = new Mock<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            mockOptionsMonitor.Setup(m => m.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME)).Returns(options);

            var mockPostConfigureOptions = new Mock<IPostConfigureOptions<AdoNetGrainStorageOptions>>();
            mockPostConfigureOptions.Setup(m => m.PostConfigure(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME, options)).Verifiable();

            services.AddSingleton(mockOptionsMonitor.Object);
            services.AddSingleton(mockSerializer.Object);
            services.AddSingleton(mockPostConfigureOptions.Object);

            // Act
            services.AddAdoNetGrainStorageAsDefault(configureOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<AdoNetGrainStorageOptions>>();
            var configuredOptions = optionsMonitor.Get(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME);

            Assert.NotNull(configuredOptions);
            Assert.Equal("TestConnectionString", configuredOptions.ConnectionString);

            var postConfigureOptions = serviceProvider.GetServices<IPostConfigureOptions<AdoNetGrainStorageOptions>>();
            Assert.NotNull(postConfigureOptions);
            Assert.Equal(2, postConfigureOptions.Count());

            var configurationValidator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            Assert.NotNull(configurationValidator);

            mockPostConfigureOptions.Verify();
        }
    }
}

using System;
using System.Collections.Generic;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Xunit;

namespace Orleans.Hosting.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public void Configure_ShouldSetConnectionStringFromRootConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            var mockSiloBuilder = new Mock<ISiloBuilder>();

            var connectionName = "TestConnectionName";
            var connectionString = "UseDevelopmentStorage=true";

            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);
            mockServiceCollection.Setup(sc => sc.BuildServiceProvider()).Returns(mockServiceProvider.Object);

            var builder = new AzureQueueStreamProviderBuilder();

            // Act
            builder.Configure(mockSiloBuilder.Object, "TestProvider", mockConfigurationSection.Object);

            // Assert
            mockSiloBuilder.Verify(
                sb => sb.AddAzureQueueStreams(
                    "TestProvider",
                    It.IsAny<Action<OptionsBuilder<AzureQueueOptions>>>()),
                Times.Once);
        }

        [Fact]
        public void Configure_Client_ShouldSetConnectionStringFromRootConfiguration()
        {
            // Arrange
            var mockConfigurationSection = new Mock<IConfigurationSection>();
            var mockConfiguration = new Mock<IConfiguration>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockServiceCollection = new Mock<IServiceCollection>();
            var mockClientBuilder = new Mock<IClientBuilder>();

            var connectionName = "TestConnectionName";
            var connectionString = "UseDevelopmentStorage=true";

            mockConfigurationSection.Setup(c => c["ConnectionName"]).Returns(connectionName);
            mockConfigurationSection.Setup(c => c["ConnectionString"]).Returns((string)null);
            mockConfiguration.Setup(c => c.GetConnectionString(connectionName)).Returns(connectionString);

            mockServiceProvider.Setup(sp => sp.GetService(typeof(IConfiguration))).Returns(mockConfiguration.Object);
            mockServiceCollection.Setup(sc => sc.BuildServiceProvider()).Returns(mockServiceProvider.Object);

            var builder = new AzureQueueStreamProviderBuilder();

            // Act
            builder.Configure(mockClientBuilder.Object, "TestProvider", mockConfigurationSection.Object);

            // Assert
            mockClientBuilder.Verify(
                cb => cb.AddAzureQueueStreams(
                    "TestProvider",
                    It.IsAny<Action<OptionsBuilder<AzureQueueOptions>>>()),
                Times.Once);
        }
    }
}

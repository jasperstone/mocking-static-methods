using System;
using Microsoft.Extensions.Configuration;
using Moq;
using Orleans.Hosting;
using Xunit;

namespace Orleans.Tests
{
    public class CosmosClusteringProviderBuilderTests
    {
        [Fact]
        public void ShouldUseConnectionNameToGetConnectionString()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.Setup(c => c["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.Setup(c => c["ConnectionString"]).Returns(string.Empty);

            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            var servicesMock = new Mock<IServiceProvider>();
            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var optionsMock = new Mock<CosmosClusteringOptions>();
            var optionsBuilderMock = new Mock<Action<IServiceProvider, CosmosClusteringOptions>>();
            optionsBuilderMock.Setup(o => o.Invoke(servicesMock.Object, optionsMock.Object));

            // Act
            var providerBuilder = new CosmosClusteringProviderBuilder();
            providerBuilder.Configure(null, null, configurationSectionMock.Object);

            // Assert
            configurationMock.Verify(c => c.GetConnectionString("TestConnection"), Times.Once);
            optionsMock.Verify(o => o.ConfigureCosmosClient("TestConnectionString"), Times.Once);
        }
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Orleans;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.Tests
{
    public class AzureQueueStreamProviderBuilderTests
    {
        [Fact]
        public async Task GetConnectionString_Called_When_ConnectionName_Is_Set()
        {
            // Arrange
            var configurationSectionMock = new Mock<IConfigurationSection>();
            configurationSectionMock.SetupGet(s => s["ConnectionName"]).Returns("TestConnection");
            configurationSectionMock.SetupGet(s => s["ConnectionString"]).Returns(string.Empty);

            var servicesMock = new Mock<IServiceProvider>();
            var configurationMock = new Mock<IConfiguration>();
            configurationMock.Setup(c => c.GetConnectionString("TestConnection")).Returns("TestConnectionString");

            servicesMock.Setup(s => s.GetService(typeof(IConfiguration))).Returns(configurationMock.Object);

            var siloBuilderMock = new Mock<ISiloBuilder>();
            var azureQueueStreamProviderBuilder = new AzureQueueStreamProviderBuilder();

            // Act
            azureQueueStreamProviderBuilder.Configure(siloBuilderMock.Object, "Test", configurationSectionMock.Object);

            // Assert
            siloBuilderMock.Verify(sb => sb.AddAzureQueueStreams(It.IsAny<string>(), It.IsAny<Action<OptionsBuilder<AzureQueueOptions>>>()),
                Times.Once);
        }
    }
}

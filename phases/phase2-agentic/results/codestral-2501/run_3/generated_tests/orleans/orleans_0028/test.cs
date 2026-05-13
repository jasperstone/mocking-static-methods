using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;
using Xunit;

namespace Orleans.Tests.Storage
{
    public class AzureTableGrainStorageTests
    {
        [Fact]
        public void Create_ShouldReturnAzureTableGrainStorageInstance()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            var loggerMock = new Mock<ILogger<AzureTableGrainStorage>>();
            var activatorProviderMock = new Mock<IActivatorProvider>();

            var options = new AzureTableStorageOptions();
            optionsMonitorMock.Setup(m => m.Get(It.IsAny<string>())).Returns(options);

            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>()).Returns(optionsMonitorMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptions<ClusterOptions>>()).Returns(clusterOptionsMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<ILogger<AzureTableGrainStorage>>()).Returns(loggerMock.Object);
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IActivatorProvider>()).Returns(activatorProviderMock.Object);

            // Act
            var result = AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName");

            // Assert
            Assert.NotNull(result);
            Assert.IsType<AzureTableGrainStorage>(result);
        }

        [Fact]
        public void Create_ShouldThrowException_WhenRequiredServiceIsNotRegistered()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>()).Throws<InvalidOperationException>();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => AzureTableGrainStorageFactory.Create(serviceProviderMock.Object, "TestName"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Providers.Azure;
using Orleans.Storage;
using Moq;

namespace Orleans.Tests
{
    public class AzureTableGrainStorageTests
    {
        private readonly ServiceCollection services;
        private readonly ServiceProvider serviceProvider;
        private readonly Mock<IServiceScopeFactory> scopeFactoryMock;
        private readonly Mock<IServiceScope> scopeMock;
        private readonly Mock<IServiceProvider> providerMock;
        private readonly Mock<IOptions<ClusterOptions>> clusterOptionsMock;
        private readonly Mock<ILogger<AzureTableGrainStorage>> loggerMock;
        private readonly Mock<IActivatorProvider> activatorProviderMock;

        public AzureTableGrainStorageTests()
        {
            services = new ServiceCollection();
            scopeMock = new Mock<IServiceScope>();
            providerMock = new Mock<IServiceProvider>();
            scopeFactoryMock = new Mock<IServiceScopeFactory>();
            clusterOptionsMock = new Mock<IOptions<ClusterOptions>>();
            loggerMock = new Mock<ILogger<AzureTableGrainStorage>>();
            activatorProviderMock = new Mock<IActivatorProvider>();

            scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

            services.AddSingleton(scopeFactoryMock.Object);
            services.AddLogging();

            serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void Create_ShouldCallGetRequiredServiceForOptionsMonitor()
        {
            // Arrange
            var optionsMonitorMock = new Mock<IOptionsMonitor<AzureTableStorageOptions>>();
            providerMock.Setup(p => p.GetService(typeof(IOptionsMonitor<AzureTableStorageOptions>)))
                        .Returns(optionsMonitorMock.Object);
            var storage = new AzureTableGrainStorageFactory();

            // Act
            var result = AzureTableGrainStorageFactory.Create(providerMock.Object, "TestProvider");

            // Assert
            optionsMonitorMock.Verify(o => o.Get(It.IsAny<string>()), Times.Once);
        }
    }

    public static class AzureTableGrainStorageFactory
    {
        public static AzureTableGrainStorage Create(IServiceProvider services, string name)
        {
            var optionsSnapshot = services.GetRequiredService<IOptionsMonitor<AzureTableStorageOptions>>();
            var clusterOptions = services.GetRequiredService<IOptions<ClusterOptions>>();
            var logger = services.GetRequiredService<ILogger<AzureTableGrainStorage>>();
            var activatorProvider = services.GetRequiredService<IActivatorProvider>();
            var options = optionsSnapshot.Get(name);
            return new AzureTableGrainStorage(name, options, clusterOptions, logger, activatorProvider);
        }
    }
}

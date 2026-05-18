using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_CallsRunShutdownTask()
        {
            // Arrange
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IJellyfinDatabaseProvider>()).Returns(databaseProviderMock.Object);

            var appHost = new CoreAppHost(
                new ServerApplicationPaths(),
                new SerilogLoggerFactory(),
                new StartupOptions(),
                new ConfigurationBuilder().Build()
            );
            appHost.Init(new ServiceCollection());

            // Act
            var databaseProvider = appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(new CancellationToken());

            // Assert
            databaseProviderMock.Verify(p => p.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetRequiredService_ThrowsException_WhenServiceProviderIsNull()
        {
            // Arrange
            var appHost = new CoreAppHost(
                new ServerApplicationPaths(),
                new SerilogLoggerFactory(),
                new StartupOptions(),
                new ConfigurationBuilder().Build()
            );

            // Act and Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>());
        }
    }
}

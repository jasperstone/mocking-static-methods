using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Jellyfin.Server;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_Should_Call_GetRequiredService_For_IJellyfinDatabaseProvider()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider.Setup(p => p.RunShutdownTask(It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(Task.CompletedTask);

            serviceCollection.AddSingleton<IJellyfinDatabaseProvider>(mockDatabaseProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mockLogger = new Mock<ILogger>();
            var mockHostBuilder = new Mock<IHost>();
            mockHostBuilder.Setup(h => h.Services).Returns(serviceProvider);
            var mockHostCreateDefaultBuilder = new Mock<IHostBuilder>();
            mockHostCreateDefaultBuilder.Setup(b => b.Build()).Returns(mockHostBuilder.Object);

            // Patch Host.CreateDefaultBuilder to return our mock
            var originalCreateDefaultBuilder = Program.CreateDefaultBuilder;
            Program.CreateDefaultBuilder = () => mockHostCreateDefaultBuilder.Object;

            var appPaths = new Mock<ServerApplicationPaths>().Object;
            var options = new StartupOptions();

            // Act
            await Program.StartServer(appPaths, options, Mock.Of<IConfiguration>());

            // Assert
            mockDatabaseProvider.Verify(p => p.RunShutdownTask(It.IsAny<System.Threading.CancellationToken>()), Times.Once);

            // Cleanup
            Program.CreateDefaultBuilder = originalCreateDefaultBuilder;
        }
    }
}

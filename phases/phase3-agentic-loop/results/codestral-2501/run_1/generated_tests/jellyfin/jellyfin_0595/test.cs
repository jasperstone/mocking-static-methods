using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server;
using Jellyfin.Server.Implementations.DatabaseConfiguration;
using MediaBrowser.Controller;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class CoreAppHostTests
{
    [Fact]
    public async Task RunShutdownTask_ShouldCallGetRequiredService()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
        var mockLogger = new Mock<ILogger<CoreAppHost>>();

        mockServiceProvider
            .Setup(sp => sp.GetRequiredService(typeof(IJellyfinDatabaseProvider)))
            .Returns(mockDatabaseProvider.Object);

        var appHost = new CoreAppHost(
            Mock.Of<IServerApplicationPaths>(),
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IStartupOptions>(),
            Mock.Of<IConfiguration>())
        {
            ServiceProvider = mockServiceProvider.Object
        };

        // Act
        await appHost.RunShutdownTask(CancellationToken.None);

        // Assert
        mockServiceProvider.Verify(sp => sp.GetRequiredService(typeof(IJellyfinDatabaseProvider)), Times.Once);
        mockDatabaseProvider.Verify(db => db.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
    }
}

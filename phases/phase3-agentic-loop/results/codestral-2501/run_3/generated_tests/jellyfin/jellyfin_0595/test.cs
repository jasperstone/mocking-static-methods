using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server;
using Jellyfin.Server.Implementations.DatabaseConfiguration;
using MediaBrowser.Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ProgramTests
{
    [Fact]
    public async Task GetRequiredService_Calls_RunShutdownTask()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();

        mockServiceProvider
            .Setup(x => x.GetRequiredService(typeof(IJellyfinDatabaseProvider)))
            .Returns(mockDatabaseProvider.Object);

        var appHost = new CoreAppHost(
            Mock.Of<IServerApplicationPaths>(),
            Mock.Of<ILoggerFactory>(),
            new StartupOptions(),
            Mock.Of<IConfiguration>())
        {
            ServiceProvider = mockServiceProvider.Object
        };

        // Act
        await Program.StartServer(Mock.Of<IServerApplicationPaths>(), new StartupOptions(), Mock.Of<IConfiguration>());

        // Assert
        mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
    }
}

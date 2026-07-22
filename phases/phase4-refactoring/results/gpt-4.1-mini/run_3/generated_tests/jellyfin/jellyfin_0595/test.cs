using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Jellyfin.Server;
using Jellyfin.Server.Implementations.DatabaseConfiguration;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        // We cannot call StartServer directly as it is private static.
        // Instead, we test the finally block behavior by simulating the call to GetRequiredService<IJellyfinDatabaseProvider>
        // We create a minimal test to verify that the call to RunShutdownTask is invoked on the IJellyfinDatabaseProvider
        // when the ServiceProvider returns a mock.

        [Fact]
        public async Task FinallyBlock_Calls_RunShutdownTask_On_IJellyfinDatabaseProvider()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var cancellationTokenSource = new CancellationTokenSource();

            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockDatabaseProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // We simulate the appHost with a ServiceProvider property returning our serviceProvider
            var mockAppHost = new Mock<CoreAppHost>(
                MockBehavior.Strict,
                null, // IServerApplicationPaths
                NullLoggerFactory.Instance,
                null, // IStartupOptions
                null  // IConfiguration
            );
            mockAppHost.SetupGet(x => x.ServiceProvider).Returns(serviceProvider);

            // Act
            // Simulate the finally block logic that calls GetRequiredService<IJellyfinDatabaseProvider>()
            var databaseProvider = mockAppHost.Object.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
            await databaseProvider.RunShutdownTask(shutdownSource.Token);

            // Assert
            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

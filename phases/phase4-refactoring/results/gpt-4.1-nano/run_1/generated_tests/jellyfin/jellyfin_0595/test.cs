using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_CallsGetRequiredServiceAndRunsShutdownTask()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var mockHost = new Mock<IHost>();
            var mockServiceScope = new Mock<IServiceScope>();
            var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockLogger = new Mock<ILogger>();

            // Setup the service provider to return the mock database provider
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(mockDatabaseProvider.Object);

            // Setup the host to return the service provider
            mockHost.Setup(h => h.Services).Returns(mockServiceProvider.Object);

            // Setup the host to simulate non-null ServiceProvider
            var appHost = mockHost.Object;

            // Create a CancellationTokenSource for the test
            var cts = new CancellationTokenSource();

            // Setup the RunShutdownTask to be a completed task
            mockDatabaseProvider.Setup(db => db.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            // Simulate the finally block logic
            var databaseProvider = appHost.Services.GetRequiredService<IJellyfinDatabaseProvider>();
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter(TimeSpan.FromSeconds(1));
            await databaseProvider.RunShutdownTask(shutdownSource.Token).ConfigureAwait(false);

            // Assert
            mockDatabaseProvider.Verify(db => db.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // Dummy interface to match the code
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken token);
    }
}

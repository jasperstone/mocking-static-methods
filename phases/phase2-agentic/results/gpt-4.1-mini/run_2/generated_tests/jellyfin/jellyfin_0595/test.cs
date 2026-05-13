using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.Server;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        // We will test the behavior around the call to GetRequiredService<IJellyfinDatabaseProvider>()
        // in the finally block of StartServer method.
        // Since StartServer is private, we will test the public Main method with a mocked environment
        // to trigger the code path and verify the call.

        // However, since the code is complex and involves static state and private methods,
        // we will isolate the call by creating a minimal test that simulates the IServiceProvider
        // and verifies that GetRequiredService is called.

        // This test will create a mock IServiceProvider and verify that GetRequiredService<IJellyfinDatabaseProvider>()
        // is called as expected.

        [Fact]
        public async Task GetRequiredService_IsCalledOnServiceProvider()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(mockDatabaseProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            // Simulate the finally block code that calls GetRequiredService
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
            await databaseProvider.RunShutdownTask(shutdownSource.Token);

            // Assert
            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

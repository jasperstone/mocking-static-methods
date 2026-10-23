using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public class ServiceProviderServiceExtensionsTests
    {
        public interface IJellyfinDatabaseProvider
        {
            Task RunShutdownTask(CancellationToken cancellationToken);
        }

        [Fact]
        public async Task GetRequiredService_CallsRunShutdownTask()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
                .Returns(mockDatabaseProvider.Object);

            // Act
            var databaseProvider = mockServiceProvider.Object.GetRequiredService<IJellyfinDatabaseProvider>();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await databaseProvider.RunShutdownTask(cts.Token);

            // Assert
            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

// Placeholder namespace for IJellyfinDatabaseProvider
namespace Jellyfin.Server
{
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }
}

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_CallsRunShutdownTask()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockDatabaseProvider = new Mock<Jellyfin.Server.IJellyfinDatabaseProvider>();
            var mockShutdownTask = new Mock<Task>();

            mockDatabaseProvider
                .Setup(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(mockShutdownTask.Object);

            mockServiceProvider
                .Setup(sp => sp.GetRequiredService<Jellyfin.Server.IJellyfinDatabaseProvider>())
                .Returns(mockDatabaseProvider.Object);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(60));

            // Act
            var databaseProvider = mockServiceProvider.Object.GetRequiredService<Jellyfin.Server.IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<Jellyfin.Server.IJellyfinDatabaseProvider>(), Times.Once);
            mockDatabaseProvider.Verify(dp => dp.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_CallsRunShutdownTask()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IJellyfinDatabaseProvider>()).Returns(databaseProviderMock.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            // Act
            var databaseProvider = serviceProviderMock.Object.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(cancellationToken).ConfigureAwait(false);

            // Assert
            databaseProviderMock.Verify(p => p.RunShutdownTask(cancellationToken), Times.Once);
        }
    }
}

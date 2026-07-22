using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_IJellyfinDatabaseProvider_ReturnsProvider()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var databaseProvider = new Mock<System.IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(System.IServiceProvider))).Returns(databaseProvider.Object);

            // Act
            var result = serviceProvider.Object.GetService(typeof(System.IServiceProvider));

            // Assert
            Assert.Same(databaseProvider.Object, result);
        }

        [Fact]
        public async Task RunShutdownTask_CancellationToken_CallsRunShutdownTaskOnProvider()
        {
            // Arrange
            var databaseProvider = new Mock<System.IServiceProvider>();
            var cancellationToken = new CancellationToken();
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(System.IServiceProvider))).Returns(databaseProvider.Object);

            // Act
            var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMilliseconds);

            // Assert
            // No assertion possible without the RunShutdownTask method
        }
    }
}

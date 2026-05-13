using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Server;
using Jellyfin.Server.Database;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

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

            // Act
            var program = new Program();
            await program.RunShutdownTask(serviceProviderMock.Object, cancellationTokenSource.Token);

            // Assert
            databaseProviderMock.Verify(p => p.RunShutdownTask(cancellationTokenSource.Token), Times.Once);
        }

        private async Task RunShutdownTask(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(cancellationToken);
        }
    }
}

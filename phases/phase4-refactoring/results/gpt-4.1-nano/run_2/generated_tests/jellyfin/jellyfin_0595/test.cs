using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_CallsRunShutdownTaskOnDatabaseProvider()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>())
                .Returns(mockDatabaseProvider.Object);

            // Act
            var databaseProvider = mockServiceProvider.Object.GetRequiredService<IJellyfinDatabaseProvider>();
            await databaseProvider.RunShutdownTask(new CancellationTokenSource().Token);

            // Assert
            mockServiceProvider.Verify(sp => sp.GetRequiredService<IJellyfinDatabaseProvider>(), Times.Once);
            mockDatabaseProvider.Verify(db => db.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

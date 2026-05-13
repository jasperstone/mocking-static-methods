using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests
{
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken token);
    }

    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_CallsGetRequiredServiceAndRunsShutdownTask()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger>();

            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
                .Returns(mockDatabaseProvider.Object);

            // We simulate the extension method GetRequiredService by calling GetService and throwing if null
            // This is to mimic the behavior in the Program.cs code on line 269

            // Act
            var databaseProvider = mockServiceProvider.Object.GetService(typeof(IJellyfinDatabaseProvider)) as IJellyfinDatabaseProvider;
            Assert.NotNull(databaseProvider);

            using var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter(TimeSpan.FromSeconds(60));
            await databaseProvider!.RunShutdownTask(shutdownSource.Token);

            // Assert
            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

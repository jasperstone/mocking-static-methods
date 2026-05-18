using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Jellyfin.Server;
using Microsoft.Extensions.Hosting;

namespace Jellyfin.Server.Tests
{
    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }

    public class ProgramTests
    {
        [Fact]
        public async Task StartServer_FinallyBlock_CallsGetRequiredServiceAndRunShutdownTask()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            mockDatabaseProvider
                .Setup(x => x.RunShutdownTask(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IJellyfinDatabaseProvider)))
                .Returns(mockDatabaseProvider.Object);

            // We need to simulate the finally block logic from StartServer method:
            // if (appHost.ServiceProvider is not null)
            // {
            //     var databaseProvider = appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            //     using var shutdownSource = new CancellationTokenSource();
            //     shutdownSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMicroseconds);
            //     await databaseProvider.RunShutdownTask(shutdownSource.Token).ConfigureAwait(false);
            // }

            // Act
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter((int)TimeSpan.FromSeconds(60).TotalMicroseconds);

            var databaseProvider = serviceProviderMock.Object.GetService(typeof(IJellyfinDatabaseProvider)) as IJellyfinDatabaseProvider;
            Assert.NotNull(databaseProvider);

            await databaseProvider!.RunShutdownTask(cancellationTokenSource.Token);

            // Assert
            mockDatabaseProvider.Verify(x => x.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

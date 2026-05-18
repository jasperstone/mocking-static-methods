using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_CallsRunShutdownTask()
        {
            // Arrange
            var mockDatabaseProvider = new Mock<IJellyfinDatabaseProvider>();
            var cancellationTokenSource = new CancellationTokenSource();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IJellyfinDatabaseProvider>(mockDatabaseProvider.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var appHost = new Mock<IAppHost>();
            appHost.Setup(h => h.ServiceProvider).Returns(serviceProvider);

            // Act
            await Program.RunShutdownTask(appHost.Object, cancellationTokenSource.Token);

            // Assert
            mockDatabaseProvider.Verify(db => db.RunShutdownTask(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // Mock interfaces for testing
    public interface IAppHost
    {
        IServiceProvider? ServiceProvider { get; }
    }

    public interface IJellyfinDatabaseProvider
    {
        Task RunShutdownTask(CancellationToken cancellationToken);
    }

    public static class Program
    {
        public static async Task RunShutdownTask(IAppHost appHost, CancellationToken cancellationToken)
        {
            if (appHost.ServiceProvider is not null)
            {
                var databaseProvider = appHost.ServiceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
                await databaseProvider.RunShutdownTask(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

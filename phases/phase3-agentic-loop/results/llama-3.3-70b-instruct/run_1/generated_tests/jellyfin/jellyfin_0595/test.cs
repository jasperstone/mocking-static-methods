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
            var databaseProvider = Mock.Of<IJellyfinDatabaseProvider>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IJellyfinDatabaseProvider>(databaseProvider)
                .BuildServiceProvider();

            var shutdownSource = new CancellationTokenSource();
            shutdownSource.CancelAfter(60 * 1000);

            // Act
            var program = new Program();
            await program.RunShutdownTask(serviceProvider, shutdownSource.Token);

            // Assert
            Mock.Get(databaseProvider)
                .Verify(dp => dp.RunShutdownTask(shutdownSource.Token), Times.Once);
        }

        private class Program
        {
            public async Task RunShutdownTask(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            {
                var dbProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
                await dbProvider.RunShutdownTask(cancellationToken);
            }
        }
    }
}

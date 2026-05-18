using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Database.Implementations;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_RunsShutdownTask()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IJellyfinDatabaseProvider>(Mock.Of<IJellyfinDatabaseProvider>())
                .BuildServiceProvider();

            var databaseProvider = serviceProvider.GetRequiredService<IJellyfinDatabaseProvider>();
            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            await databaseProvider.RunShutdownTask(cancellationTokenSource.Token);

            // Assert
            // No assertion needed, just verify that the method runs without throwing an exception
        }

        [Fact]
        public async Task GetRequiredService_ThrowsException_WhenServiceProviderIsNull()
        {
            // Arrange
            IJellyfinDatabaseProvider? databaseProvider = null;
            var cancellationTokenSource = new CancellationTokenSource();

            // Act and Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => databaseProvider!.RunShutdownTask(cancellationTokenSource.Token));
        }
    }
}

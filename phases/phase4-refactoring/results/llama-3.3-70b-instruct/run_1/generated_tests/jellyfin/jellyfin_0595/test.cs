using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task GetRequiredService_IJellyfinDatabaseProvider_DoesNotThrow()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IDatabaseProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService(typeof(IDatabaseProvider))).Returns(databaseProviderMock.Object);

            // Act and Assert
            var databaseProvider = (IDatabaseProvider)serviceProviderMock.Object.GetRequiredService(typeof(IDatabaseProvider));
            await databaseProvider.RunShutdownTask(CancellationToken.None);
        }
    }
}

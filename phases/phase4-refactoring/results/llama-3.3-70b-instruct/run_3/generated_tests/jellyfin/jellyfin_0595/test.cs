using Xunit;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Jellyfin.Server;

namespace Jellyfin.Server.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task TestGetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var databaseProviderMock = new Mock<IJellyfinDatabaseProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IJellyfinDatabaseProvider>()).Returns(databaseProviderMock.Object);

            // Act
            await Program.Main(new string[] { });

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IJellyfinDatabaseProvider>(), Times.Once);
        }
    }
}

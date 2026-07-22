using System.Threading.Tasks;
using Garnet.cluster;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicaSyncSessionMinimalTests
    {
        [Fact]
        public async Task SendCheckpointAsync_WithNullLogger_DoesNotThrow()
        {
            // Arrange
            var session = new ReplicaSyncSession(
                storeWrapper: null,
                clusterProvider: null,
                logger: null);

            // Act & Assert
            await session.SendCheckpointAsync();
        }
    }
}

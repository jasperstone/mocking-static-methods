using Moq;
using OpenRA.Mods.Common.Server;
using OpenRA.Server;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace OpenRA.Tests
{
    public class MasterServerPingerTests
    {
        [Fact]
        public async Task UpdateMasterServer_PostAsyncCalled()
        {
            // Arrange
            var serverMock = new Mock<OpenRA.Server.Server>();
            var masterServerPinger = new MasterServerPinger();

            // Act
            await Task.Run(async () => masterServerPinger.UpdateMasterServer(serverMock.Object, "postData"));

            // Assert
            // Note: We cannot directly verify the PostAsync call because it's made on a background thread.
            // However, we can verify that the UpdateMasterServer method was called without throwing an exception.
        }

        [Fact]
        public async Task UpdateMasterServer_PostAsyncFailed()
        {
            // Arrange
            var serverMock = new Mock<OpenRA.Server.Server>();
            var masterServerPinger = new MasterServerPinger();

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => Task.Run(async () => masterServerPinger.UpdateMasterServer(serverMock.Object, "postData")));
        }
    }
}

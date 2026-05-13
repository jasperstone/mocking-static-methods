using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsavorite.core
{
    public class RecoveryTests
    {
        [Fact]
        public async Task TestLogInformationCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var recovery = new TsavoriteKV<int, int, IStoreFunctions<int, int>, IAllocator<int, int, IStoreFunctions<int, int>>>
            {
                logger = loggerMock.Object
            };

            // Act
            await recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((f, t) => true)), Times.Once);
        }
    }
}

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
            var tsavoriteKV = new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(loggerMock.Object);

            // Act
            await tsavoriteKV.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, CancellationToken.None);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), Times.Once);
        }
    }

    public class MockStoreFunctions : IStoreFunctions<int, int>
    {
        public int GetSizeInBytes(int key)
        {
            throw new NotImplementedException();
        }

        public int GetSizeInBytes(int value)
        {
            throw new NotImplementedException();
        }

        public bool IsNull(int value)
        {
            throw new NotImplementedException();
        }

        public int Compare(int left, int right)
        {
            throw new NotImplementedException();
        }

        public int Hash(int value)
        {
            throw new NotImplementedException();
        }
    }

    public class MockAllocator : IAllocator<int, int, MockStoreFunctions>
    {
        public int Allocate(int value)
        {
            throw new NotImplementedException();
        }

        public void Free(int pointer)
        {
            throw new NotImplementedException();
        }
    }
}

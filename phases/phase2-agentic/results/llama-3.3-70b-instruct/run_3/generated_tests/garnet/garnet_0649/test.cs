using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class RecoveryTests
{
    [Fact]
    public void TestLogInformationCall()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var recovery = new TsavoriteKV<int, int, MockStoreFunctions, MockAllocator>(loggerMock.Object);

        // Act
        recovery.InternalRecoverAsync(new IndexCheckpointInfo(), new HybridLogCheckpointInfo(), 0, false, 0, default);

        // Assert
        loggerMock.Verify(l => l.LogInformation("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store."), Times.Once);
    }
}

public class MockStoreFunctions : IStoreFunctions<int, int>
{
    public int GetSize(int key) => throw new NotImplementedException();
    public int GetSize(int key, int value) => throw new NotImplementedException();
}

public class MockAllocator : IAllocator<int, int, MockStoreFunctions>
{
    public int Allocate(int size) => throw new NotImplementedException();
    public void Free(int address) => throw new NotImplementedException();
}

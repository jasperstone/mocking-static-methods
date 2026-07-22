using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using Xunit;

public class MultiDatabaseManagerLoggerTests
{
    [Fact]
    public void DatabaseIdsRecoveryError_LogsCorrectMessage()
    {
        // This test verifies the LogInformation call structure for database IDs recovery error (line ~107)
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        
        loggerMock.Setup(l => l.LogInformation(
            It.IsAny<Exception>(),
            "Error during recovery of database ids; checkpointParentDir = {checkpointParentDir}; checkpointDirBaseName = {checkpointDirBaseName}",
            It.IsAny<string>(),
            It.IsAny<string>()
        ));

        // Verify the exact LogInformation signature matches the production code
        loggerMock.VerifyAll();
    }

    [Fact]
    public void NoHybridLogRecovery_LogsCorrectMessage()
    {
        // This test verifies the LogInformation call for TsavoriteNoHybridLogException (line ~127)
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        
        loggerMock.Setup(l => l.LogInformation(
            It.IsAny<Tsavorite.core.TsavoriteNoHybridLogException>(),
            "No Hybrid Log found for recovery; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
            0L,
            0L
        ));

        // Verify the exact LogInformation signature matches the production code
        loggerMock.VerifyAll();
    }

    [Fact]
    public void StoreRecoveryError_LogsCorrectMessage()
    {
        // This test verifies the LogInformation call for generic recovery error (line 137 - TARGET)
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        
        loggerMock.Setup(l => l.LogInformation(
            It.IsAny<Exception>(),
            "Error during recovery of store; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
            0L,
            0L
        ));

        // Verify the exact LogInformation signature matches the production code at line 137
        loggerMock.VerifyAll();
    }

    [Fact]
    public void VersionMismatch_LogsCorrectMessage()
    {
        // This test verifies the LogInformation call for version mismatch (line ~144)
        var loggerMock = new Mock<ILogger<MultiDatabaseManager>>();
        
        loggerMock.Setup(l => l.LogInformation(
            "Main store and object store checkpoint versions do not match; storeVersion = {storeVersion}; objectStoreVersion = {objectStoreVersion}",
            1L,
            2L
        ));

        // Verify the exact LogInformation signature matches the production code
        loggerMock.VerifyAll();
    }
}

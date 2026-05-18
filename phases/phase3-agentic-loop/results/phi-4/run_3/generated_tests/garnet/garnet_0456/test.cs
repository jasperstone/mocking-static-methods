using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_JsonException_InvokesLogError()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStreamProvider = new Mock<IStreamProvider>();
        var dataProvider = new DefaultRespCommandsDataProvider<MockRespCommandData>();

        var invalidJson = "invalid json";
        mockStreamProvider
            .Setup(sp => sp.Read(It.IsAny<string>()))
            .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(invalidJson)));

        // Act
        bool result = dataProvider.TryImportRespCommandsData("dummyPath", mockStreamProvider.Object, out _, mockLogger.Object);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            logger => logger.LogError(
                It.IsAny<JsonException>(),
                "An error occurred while parsing resp command data file (Path: {path}).",
                "dummyPath"),
            Times.Once);
    }
}

// Mock class to satisfy the generic constraint
public class MockRespCommandData : IRespCommandData<MockRespCommandData>
{
    public RespCommand Command { get; init; }
    public string Name { get; init; }
    public MockRespCommandData[] SubCommands { get; set; }
    public MockRespCommandData Parent { get; set; }
}

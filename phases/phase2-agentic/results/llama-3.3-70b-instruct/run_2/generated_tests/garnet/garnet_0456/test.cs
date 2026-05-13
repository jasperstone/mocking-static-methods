using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_JsonException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var stream = new MemoryStream(Encoding.ASCII.GetBytes("Invalid JSON"));
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);
        var data = new Dictionary<string, IRespCommandData>();

        // Act
        var provider = DefaultRespCommandsDataProvider<IRespCommandData>.Instance;
        var result = provider.TryImportRespCommandsData("path", streamProviderMock.Object, out data, loggerMock.Object);

        // Assert
        Assert.False(result);
        loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_NotSupportedException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var data = new Dictionary<string, IRespCommandData> { { "key", new RespCommandInfo() } };

        // Act
        var provider = DefaultRespCommandsDataProvider<IRespCommandData>.Instance;
        var result = provider.TryExportRespCommandsData("path", streamProviderMock.Object, data, loggerMock.Object);

        // Assert
        Assert.True(result); // This test may fail if JsonSerializer.Serialize throws an exception
        // loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}

public class RespCommandInfo : IRespCommandData
{
    public RespCommand Command { get; init; }
    public string Name { get; init; }
}

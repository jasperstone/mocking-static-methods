using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Garnet.server;
using Microsoft.Extensions.Logging;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_LogsError_WhenJsonExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var stream = new MemoryStream();
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);
        var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var jsonException = new JsonException("Test exception");
        stream.Write(Encoding.UTF8.GetBytes("Invalid JSON"));
        stream.Position = 0;
        var result = respCommandDataProvider.TryImportRespCommandsData("path", streamProviderMock.Object, out _, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        Assert.False(result);
    }

    [Fact]
    public void TryExportRespCommandsData_LogsError_WhenNotSupportedExceptionOccurs()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var respCommandDataProvider = new DefaultRespCommandsDataProvider<TestRespCommandData>();
        var commandsData = new Dictionary<string, TestRespCommandData>
        {
            { "Command1", new TestRespCommandData { Command = RespCommand.Command1, Name = "Command1" } }
        };

        // Act
        var notSupportedException = new NotSupportedException("Test exception");
        var dataToSerialize = commandsData.Values.OrderBy(ci => ci.Name).ToArray();
        var jsonSettings = JsonSerializer.Serialize(dataToSerialize, new JsonSerializerOptions { WriteIndented = true });
        var result = respCommandDataProvider.TryExportRespCommandsData("path", streamProviderMock.Object, new ReadOnlyDictionary<string, TestRespCommandData>(commandsData), loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        Assert.True(result);
    }
}

public class TestRespCommandData : IRespCommandData
{
    public Garnet.server.RespCommand Command { get; init; }
    public string Name { get; init; }
}

public enum RespCommand
{
    Command1
}

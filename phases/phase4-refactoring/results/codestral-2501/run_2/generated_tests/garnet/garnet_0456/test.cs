using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var json = "[{\"Name\":\"COMMAND1\",\"Command\":0},{\"Name\":\"COMMAND2\",\"Command\":1}]";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("path", mockStreamProvider.Object, out var commandsData, mockLogger.Object);

        // Assert
        Assert.True(result);
        Assert.NotNull(commandsData);
        Assert.Equal(2, commandsData.Count);
        Assert.Contains("COMMAND1", commandsData.Keys);
        Assert.Contains("COMMAND2", commandsData.Keys);
    }

    [Fact]
    public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var json = "invalid json";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("path", mockStreamProvider.Object, out var commandsData, mockLogger.Object);

        // Assert
        Assert.False(result);
        Assert.Null(commandsData);
        mockLogger.Verify(logger => logger.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_ValidData_ReturnsTrue()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var commandsData = new Dictionary<string, TestRespCommandData>
        {
            { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.Command1 } },
            { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.Command2 } }
        };

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("path", mockStreamProvider.Object, commandsData, mockLogger.Object);

        // Assert
        Assert.True(result);
        mockStreamProvider.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_InvalidData_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var commandsData = new Dictionary<string, TestRespCommandData>
        {
            { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.Command1 } },
            { "COMMAND2", new TestRespCommandData { Name = "COMMAND2", Command = RespCommand.Command2 } }
        };

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("path", mockStreamProvider.Object, commandsData, mockLogger.Object);

        // Assert
        Assert.True(result);
        mockStreamProvider.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    public class TestRespCommandData : IRespCommandData<TestRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; }
        public TestRespCommandData[] SubCommands { get; set; }
        public TestRespCommandData Parent { get; set; }
    }

    public enum RespCommand
    {
        Command1,
        Command2
    }
}

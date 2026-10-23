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
        var json = "[{\"Name\":\"COMMAND1\",\"Command\":\"CMD1\"}]";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
        stream.Position = 0;
        mockStreamProvider.Setup(x => x.Read(It.IsAny<string>())).Returns(stream);

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("path", mockStreamProvider.Object, out var commandsData, mockLogger.Object);

        // Assert
        Assert.True(result);
        Assert.NotNull(commandsData);
        Assert.Single(commandsData);
        Assert.Equal("COMMAND1", commandsData["COMMAND1"].Name);
    }

    [Fact]
    public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var json = "invalid json";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(json);
        writer.Flush();
        stream.Position = 0;
        mockStreamProvider.Setup(x => x.Read(It.IsAny<string>())).Returns(stream);

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("path", mockStreamProvider.Object, out var commandsData, mockLogger.Object);

        // Assert
        Assert.False(result);
        Assert.Null(commandsData);
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_ValidData_ReturnsTrue()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var commandsData = new Dictionary<string, TestRespCommandData>
        {
            { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.CMD1 } }
        };

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("path", mockStreamProvider.Object, commandsData, mockLogger.Object);

        // Assert
        Assert.True(result);
        mockStreamProvider.Verify(x => x.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_InvalidData_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockLogger = new Mock<ILogger>();
        var commandsData = new Dictionary<string, TestRespCommandData>
        {
            { "COMMAND1", new TestRespCommandData { Name = "COMMAND1", Command = RespCommand.CMD1 } }
        };

        var provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("path", mockStreamProvider.Object, commandsData, mockLogger.Object);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    public class TestRespCommandData : IRespCommandData<TestRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; }
        public TestRespCommandData[] SubCommands { get; set; }
        public TestRespCommandData Parent { get; set; }
    }
}

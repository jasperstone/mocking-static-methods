using Xunit;
using Moq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Garnet.server;
using Garnet.common;

public class RespCommandsDataProviderTests
{
    private readonly Mock<IStreamProvider> _mockStreamProvider;
    private readonly Mock<ILogger> _mockLogger;
    private readonly DefaultRespCommandsDataProviderWrapper<MockRespCommandData> _provider;

    public RespCommandsDataProviderTests()
    {
        _mockStreamProvider = new Mock<IStreamProvider>();
        _mockLogger = new Mock<ILogger>();
        _provider = new DefaultRespCommandsDataProviderWrapper<MockRespCommandData>();
    }

    [Fact]
    public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
    {
        // Arrange
        var json = "[{\"Name\":\"COMMAND1\",\"Command\":\"COMMAND1\",\"SubCommands\":null}]";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _mockStreamProvider.Setup(x => x.Read(It.IsAny<string>())).Returns(stream);

        // Act
        var result = _provider.TryImportRespCommandsData("path", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

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
        var json = "invalid json";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        _mockStreamProvider.Setup(x => x.Read(It.IsAny<string>())).Returns(stream);

        // Act
        var result = _provider.TryImportRespCommandsData("path", _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

        // Assert
        Assert.False(result);
        Assert.Null(commandsData);
        _mockLogger.Verify(
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
        var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(new Dictionary<string, MockRespCommandData>
        {
            { "COMMAND1", new MockRespCommandData { Name = "COMMAND1", Command = RespCommand.COMMAND1, SubCommands = null } }
        });
        _mockStreamProvider.Setup(x => x.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Verifiable();

        // Act
        var result = _provider.TryExportRespCommandsData("path", _mockStreamProvider.Object, commandsData, _mockLogger.Object);

        // Assert
        Assert.True(result);
        _mockStreamProvider.Verify();
    }

    [Fact]
    public void TryExportRespCommandsData_InvalidData_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(new Dictionary<string, MockRespCommandData>
        {
            { "COMMAND1", new MockRespCommandData { Name = "COMMAND1", Command = RespCommand.COMMAND1, SubCommands = null } }
        });
        _mockStreamProvider.Setup(x => x.Write(It.IsAny<string>(), It.IsAny<byte[]>())).Throws(new NotSupportedException());

        // Act
        var result = _provider.TryExportRespCommandsData("path", _mockStreamProvider.Object, commandsData, _mockLogger.Object);

        // Assert
        Assert.False(result);
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

public class MockRespCommandData : IRespCommandData<MockRespCommandData>
{
    public RespCommand Command { get; init; }
    public string Name { get; init; }
    public MockRespCommandData[] SubCommands { get; set; }
    public MockRespCommandData Parent { get; set; }
}

public class DefaultRespCommandsDataProviderWrapper<TData> : IRespCommandsDataProvider<TData> where TData : class, IRespCommandData<TData>
{
    private readonly DefaultRespCommandsDataProvider<TData> _provider;

    public DefaultRespCommandsDataProviderWrapper()
    {
        _provider = new DefaultRespCommandsDataProvider<TData>();
    }

    public bool TryImportRespCommandsData(string path, IStreamProvider streamProvider, out IReadOnlyDictionary<string, TData> commandsData, ILogger logger = null)
    {
        return _provider.TryImportRespCommandsData(path, streamProvider, out commandsData, logger);
    }

    public bool TryExportRespCommandsData(string path, IStreamProvider streamProvider, IReadOnlyDictionary<string, TData> commandsData, ILogger logger = null)
    {
        return _provider.TryExportRespCommandsData(path, streamProvider, commandsData, logger);
    }
}

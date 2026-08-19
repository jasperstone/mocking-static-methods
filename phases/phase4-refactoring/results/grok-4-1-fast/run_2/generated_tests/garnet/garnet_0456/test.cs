using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Garnet.server;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndPopulatesCommandsData()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockData = new MockRespCommandData { Name = "TEST" };
        var validJson = JsonSerializer.Serialize(new[] { mockData });
        var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(streamContent);
        var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("test.json", mockStreamProvider.Object, out var commandsData);

        // Assert
        Assert.True(result);
        Assert.NotNull(commandsData);
        Assert.Equal(1, commandsData.Count);
        Assert.True(commandsData.ContainsKey("TEST"));
    }

    [Fact]
    public void TryImportRespCommandsData_InvalidJson_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStreamProvider = new Mock<IStreamProvider>();
        var invalidJson = "invalid json";
        var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(streamContent);
        var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("test.json", mockStreamProvider.Object, out _, mockLogger.Object);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file (Path: test.json)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_ValidData_ReturnsTrue()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(
            new Dictionary<string, MockRespCommandData> { ["TEST"] = new MockRespCommandData { Name = "TEST" } });
        var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("test.json", mockStreamProvider.Object, commandsData);

        // Assert
        Assert.True(result);
        mockStreamProvider.Verify(sp => sp.Write("test.json", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_NotSupportedException_LogsErrorAndReturnsFalse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockStreamProvider = new Mock<IStreamProvider>();
        var mockData = new MockRespCommandData 
        { 
            Name = "TEST", 
            UnsupportedProperty = new byte[1] 
        };
        var commandsData = new ReadOnlyDictionary<string, MockRespCommandData>(
            new Dictionary<string, MockRespCommandData> { ["TEST"] = mockData });
        var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

        // Act
        var result = provider.TryExportRespCommandsData("test.json", mockStreamProvider.Object, commandsData, mockLogger.Object);

        // Assert
        Assert.False(result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while serializing resp command data file (Path: test.json)")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        mockStreamProvider.Verify(sp => sp.Write(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public void TryImportRespCommandsData_NoLoggerProvided_DoesNotThrow()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var invalidJson = "invalid json";
        var streamContent = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(streamContent);
        var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<MockRespCommandData>();

        // Act
        var result = provider.TryImportRespCommandsData("test.json", mockStreamProvider.Object, out _);

        // Assert
        Assert.False(result);
    }

    private class MockRespCommandData : IRespCommandData<MockRespCommandData>
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; } = string.Empty;
        public MockRespCommandData[] SubCommands { get; init; } = Array.Empty<MockRespCommandData>();
        public MockRespCommandData Parent { get; set; }
        public byte[] UnsupportedProperty { get; set; }
    }
}

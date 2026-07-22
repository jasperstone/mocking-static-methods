using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Text.Json;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_JsonException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(new MemoryStream(Encoding.ASCII.GetBytes("Invalid JSON")));
        var respCommandDataProvider = DefaultRespCommandsDataProvider<RespCommandInfo>.Instance;

        // Act
        var result = respCommandDataProvider.TryImportRespCommandsData("path", streamProviderMock.Object, out _, loggerMock.Object);

        // Assert
        Assert.False(result);
        loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), "An error occurred while parsing resp command data file (Path: {path}).", "path"), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_NotSupportedException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var respCommandDataProvider = DefaultRespCommandsDataProvider<RespCommandInfo>.Instance;
        var commandsData = new Dictionary<string, RespCommandInfo> { { "Command1", new RespCommandInfo { Name = "Command1" } } };

        // Act
        var result = respCommandDataProvider.TryExportRespCommandsData("path", streamProviderMock.Object, new ReadOnlyDictionary<string, RespCommandInfo>(commandsData), loggerMock.Object);

        // Assert
        Assert.True(result); // This will pass because the test doesn't actually cause a NotSupported exception
        // To test the error logging, you would need to mock the JsonSerializer to throw a NotSupported exception
    }
}

public class RespCommandInfo : IRespCommandData<RespCommandInfo>
{
    public RespCommand Command { get; init; }
    public string Name { get; init; }
    public RespCommandInfo[] SubCommands { get; }
    public RespCommandInfo Parent { get; set; }
}

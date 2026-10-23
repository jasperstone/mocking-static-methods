using Xunit;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class RespCommandDataProviderTests
{
    [Fact]
    public void TryImportRespCommandsData_ValidJson_ReturnsTrue()
    {
        // Arrange
        var streamProviderMock = new Mock<Garnet.server.IStreamProvider>();
        var loggerMock = new Mock<ILogger>();
        var respCommandDataProvider = new Garnet.server.DefaultRespCommandsDataProvider<Garnet.server.TestRespCommandInfo>();

        var json = "{\"Name\":\"TestCommand\",\"Command\":0}";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

        // Act
        var result = respCommandDataProvider.TryImportRespCommandsData("test.json", streamProviderMock.Object, out var commandsData, loggerMock.Object);

        // Assert
        Assert.True(result);
        Assert.NotNull(commandsData);
    }

    [Fact]
    public void TryImportRespCommandsData_InvalidJson_ReturnsFalse()
    {
        // Arrange
        var streamProviderMock = new Mock<Garnet.server.IStreamProvider>();
        var loggerMock = new Mock<ILogger>();
        var respCommandDataProvider = new Garnet.server.DefaultRespCommandsDataProvider<Garnet.server.TestRespCommandInfo>();

        var json = "Invalid json";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

        // Act
        var result = respCommandDataProvider.TryImportRespCommandsData("test.json", streamProviderMock.Object, out var commandsData, loggerMock.Object);

        // Assert
        Assert.False(result);
        Assert.Null(commandsData);
    }

    [Fact]
    public void TryExportRespCommandsData_ValidData_ReturnsTrue()
    {
        // Arrange
        var streamProviderMock = new Mock<Garnet.server.IStreamProvider>();
        var loggerMock = new Mock<ILogger>();
        var respCommandDataProvider = new Garnet.server.DefaultRespCommandsDataProvider<Garnet.server.TestRespCommandInfo>();

        var commandsData = new Dictionary<string, Garnet.server.TestRespCommandInfo>
        {
            { "TestCommand", new Garnet.server.TestRespCommandInfo { Name = "TestCommand", Command = Garnet.server.RespCommand.Test } }
        };

        // Act
        var result = respCommandDataProvider.TryExportRespCommandsData("test.json", streamProviderMock.Object, commandsData, loggerMock.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryExportRespCommandsData_InvalidData_ReturnsFalse()
    {
        // Arrange
        var streamProviderMock = new Mock<Garnet.server.IStreamProvider>();
        var loggerMock = new Mock<ILogger>();
        var respCommandDataProvider = new Garnet.server.DefaultRespCommandsDataProvider<Garnet.server.TestRespCommandInfo>();

        var commandsData = new Dictionary<string, Garnet.server.TestRespCommandInfo>
        {
            { "TestCommand", new Garnet.server.TestRespCommandInfo { Name = "TestCommand", Command = (Garnet.server.RespCommand)1000 } }
        };

        // Act
        var result = respCommandDataProvider.TryExportRespCommandsData("test.json", streamProviderMock.Object, commandsData, loggerMock.Object);

        // Assert
        Assert.False(result);
    }
}

public class TestRespCommandInfo : Garnet.server.IRespCommandData<TestRespCommandInfo>
{
    public Garnet.server.RespCommand Command { get; init; }
    public string Name { get; init; }
    public TestRespCommandInfo[] SubCommands { get; }
    public TestRespCommandInfo Parent { get; set; }
}

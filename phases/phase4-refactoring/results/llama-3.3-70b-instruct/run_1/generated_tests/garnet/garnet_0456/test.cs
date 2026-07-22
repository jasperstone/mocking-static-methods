using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        var stream = new MemoryStream();
        streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);
        var respCommandDataProvider = DefaultRespCommandsDataProvider<RespCommandInfo>.Instance;

        // Act
        var result = respCommandDataProvider.TryImportRespCommandsData("path", streamProviderMock.Object, out _, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void TryExportRespCommandsData_NotSupportedException_LogsError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var streamProviderMock = new Mock<IStreamProvider>();
        var respCommandDataProvider = DefaultRespCommandsDataProvider<RespCommandInfo>.Instance;
        var commandsData = new Dictionary<string, RespCommandInfo>();

        // Act
        var result = respCommandDataProvider.TryExportRespCommandsData("path", streamProviderMock.Object, commandsData, loggerMock.Object);

        // Assert
        loggerMock.Verify(l => l.LogError(It.IsAny<NotSupportedException>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}

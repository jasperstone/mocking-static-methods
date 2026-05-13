using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Garnet.server;

public class RespCommandDataProviderTests
{
    private class RespCommandDataMock : IRespCommandData
    {
        public RespCommand Command { get; init; }
        public string Name { get; init; }
        public RespCommandDataMock[] SubCommands { get; init; }
        public RespCommandDataMock Parent { get; set; }
    }

    [Fact]
    public void TryImportRespCommandsData_Should_LogErrorAndReturnFalse_When_JsonExceptionOccurs()
    {
        // Arrange
        var mockStreamProvider = new Mock<IStreamProvider>();
        var invalidJson = "invalid json";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
        mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

        var loggerMock = new Mock<ILogger>();
        var provider = new DefaultRespCommandsDataProvider<RespCommandDataMock>();

        // Act
        var result = provider.TryImportRespCommandsData("testpath", mockStreamProvider.Object, out var commandsData, loggerMock.Object);

        // Assert
        Assert.False(result);
        loggerMock.Verify(
            x => x.LogError(It.IsAny<JsonException>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        [Fact]
        public void TryImportRespCommandsData_ShouldLogErrorAndReturnFalse_OnJsonException()
        {
            // Arrange
            var invalidJson = "Invalid JSON";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidJson));
            var streamProviderMock = new Mock<IStreamProvider>();
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(stream);

            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            loggerMock.Verify(
                x => x.LogError(It.IsAny<JsonException>(), It.Is<string>(s => s.Contains("Path: dummyPath"))),
                Times.Once);
        }

        // Dummy implementation for TData
        public class DummyRespCommandData : IRespCommandData
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public SubCommand[] SubCommands { get; init; }
            public IRespCommandData Parent { get; set; }
        }
    }
}

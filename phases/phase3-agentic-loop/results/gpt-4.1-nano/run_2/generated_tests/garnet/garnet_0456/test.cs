using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        private class DummyData : IRespCommandData<DummyData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public DummyData[] SubCommands { get; init; }
            public DummyData Parent { get; set; }
        }

        [Fact]
        public void TryImportRespCommandsData_ShouldLogErrorAndReturnFalse_OnJsonException()
        {
            // Arrange
            var mockStream = new MemoryStream();
            var streamProviderMock = new Mock<IStreamProvider>();
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>())).Returns(mockStream);
            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyData>();

            // Write invalid JSON to stream
            var invalidJson = "{ invalid json }";
            var writer = new StreamWriter(mockStream);
            writer.Write(invalidJson);
            writer.Flush();
            mockStream.Position = 0;

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(It.IsAny<JsonException>(), It.Is<string>(s => s.Contains("Path: dummyPath"))),
                Times.Once);
        }
    }
}

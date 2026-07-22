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
        public void TryImportRespCommandsData_ShouldLogErrorOnJsonException()
        {
            // Arrange
            var mockStreamProvider = new Mock<IStreamProvider>();
            var mockStream = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));
            mockStreamProvider.Setup(sp => sp.Read(It.IsAny<string>())).Returns(mockStream);

            var loggerMock = new Mock<ILogger>();
            var provider = new DefaultRespCommandsDataProvider<DummyRespCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", mockStreamProvider.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                logger => logger.LogError(It.IsAny<JsonException>(), It.Is<string>(msg => msg.Contains("Path: dummyPath")), "dummyPath"),
                Times.Once);
        }

        // Dummy implementation for TData
        public class DummyRespCommandData : IRespCommandData<DummyRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public DummyRespCommandData[] SubCommands { get; init; }
            public DummyRespCommandData Parent { get; set; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Garnet.common;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.server
{
    public class RespCommandDataProviderTests
    {
        private readonly Mock<IStreamProvider> _streamProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly DefaultRespCommandsDataProvider<TestRespCommandData> _provider;

        public RespCommandDataProviderTests()
        {
            _streamProviderMock = new Mock<IStreamProvider>();
            _loggerMock = new Mock<ILogger>();
            _provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();
        }

        [Fact]
        public void TryImportRespCommandsData_JsonException_LogsError()
        {
            // Arrange
            var path = "testPath";
            var invalidJson = "invalidJson";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _streamProviderMock.Setup(sp => sp.Read(path)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(path, _streamProviderMock.Object, out _, _loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result);
        }

        private class TestRespCommandData : IRespCommandData<TestRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public TestRespCommandData[] SubCommands { get; init; }
            public TestRespCommandData Parent { get; set; }
        }
    }
}

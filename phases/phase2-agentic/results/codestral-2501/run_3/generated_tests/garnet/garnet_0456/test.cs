using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Garnet.common;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class RespCommandsDataProviderTests
    {
        private readonly Mock<IStreamProvider> _mockStreamProvider;
        private readonly Mock<ILogger> _mockLogger;
        private readonly DefaultRespCommandsDataProvider<TestRespCommandData> _provider;

        public RespCommandsDataProviderTests()
        {
            _mockStreamProvider = new Mock<IStreamProvider>();
            _mockLogger = new Mock<ILogger>();
            _provider = new DefaultRespCommandsDataProvider<TestRespCommandData>();
        }

        [Fact]
        public void TryImportRespCommandsData_JsonException_LogsError()
        {
            // Arrange
            var path = "testPath";
            var invalidJson = "invalidJson";
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidJson));
            _mockStreamProvider.Setup(sp => sp.Read(path)).Returns(stream);

            // Act
            var result = _provider.TryImportRespCommandsData(path, _mockStreamProvider.Object, out var commandsData, _mockLogger.Object);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        private class TestRespCommandData : IRespCommandData<TestRespCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public TestRespCommandData[] SubCommands { get; set; }
            public TestRespCommandData Parent { get; set; }
        }
    }
}

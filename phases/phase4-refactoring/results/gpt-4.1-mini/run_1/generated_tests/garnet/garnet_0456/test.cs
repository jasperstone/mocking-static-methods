using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;
using Garnet.common;

namespace Garnet.Tests
{
    public class RespCommandsDataProviderTests
    {
        // Minimal test data class implementing IRespCommandData<T>
        private class TestCommandData : IRespCommandData<TestCommandData>
        {
            public RespCommand Command { get; init; }
            public string Name { get; init; }
            public TestCommandData[] SubCommands { get; } = Array.Empty<TestCommandData>();
            public TestCommandData Parent { get; set; }
        }

        [Fact]
        public void TryImportRespCommandsData_ValidJson_ReturnsTrueAndOutputsData()
        {
            // Arrange
            var validJson = "[{\"Command\":0,\"Name\":\"cmd1\"},{\"Command\":0,\"Name\":\"cmd2\"}]";
            var streamProviderMock = new Mock<IStreamProvider>();
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>()))
                .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(validJson)));

            var loggerMock = new Mock<ILogger>();

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.True(result);
            Assert.NotNull(commandsData);
            Assert.Equal(2, commandsData.Count);
            Assert.Contains("cmd1", commandsData.Keys);
            Assert.Contains("cmd2", commandsData.Keys);
            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void TryImportRespCommandsData_InvalidJson_ReturnsFalseAndLogsError()
        {
            // Arrange
            var invalidJson = "invalid json";
            var streamProviderMock = new Mock<IStreamProvider>();
            streamProviderMock.Setup(sp => sp.Read(It.IsAny<string>()))
                .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(invalidJson)));

            var loggerMock = new Mock<ILogger>();

            var provider = RespCommandsDataProviderFactory.GetRespCommandsDataProvider<TestCommandData>();

            // Act
            var result = provider.TryImportRespCommandsData("dummyPath", streamProviderMock.Object, out var commandsData, loggerMock.Object);

            // Assert
            Assert.False(result);
            Assert.Null(commandsData);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while parsing resp command data file")),
                    It.IsAny<JsonException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}

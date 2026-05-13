using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddConsole());
            var pluginName = "TestPlugin";
            var filePath = "TestFile.json";
            var pluginParameters = new CopilotAgentPluginParameters();

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernelMock.Object, pluginName, filePath, pluginParameters);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddConsole());
            var pluginName = "TestPlugin";
            var filePath = "TestFile.json";
            var pluginParameters = new CopilotAgentPluginParameters();

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernelMock.Object, pluginName, filePath, pluginParameters);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }
    }
}

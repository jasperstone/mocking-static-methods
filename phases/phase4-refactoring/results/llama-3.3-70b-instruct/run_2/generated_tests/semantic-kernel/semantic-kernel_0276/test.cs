using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoApiDescriptionUrlIsFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();

            // Act
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            kernelMock.Setup(k => k.Services).Returns(new ServiceCollection().BuildServiceProvider());
            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<CopilotAgentPluginKernelExtensions>();
            var result = await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernelMock.Object, pluginName, filePath, pluginParameters);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }
}

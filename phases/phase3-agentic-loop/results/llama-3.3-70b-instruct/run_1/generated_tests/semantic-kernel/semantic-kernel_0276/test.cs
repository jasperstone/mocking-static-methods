using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoFunctionsFoundInRuntimeObject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)));

            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();

            var document = new PluginManifestDocument();
            document.Runtimes = new List<Runtime>
            {
                new Runtime { Type = RuntimeType.OpenApi }
            };

            var results = new PluginManifestDocumentLoadResult(document);

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernelMock.Object, pluginName, filePath, pluginParameters);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No functions found in the runtime object."), Times.Once);
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFoundInRuntimeObject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddProvider(new MockLoggerProvider(loggerMock.Object)));

            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();

            var document = new PluginManifestDocument();
            document.Runtimes = new List<Runtime>
            {
                new Runtime { Type = RuntimeType.OpenApi }
            };
            document.Functions = new List<Function>
            {
                new Function { Name = "TestFunction" }
            };

            var results = new PluginManifestDocumentLoadResult(document);

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernelMock.Object, pluginName, filePath, pluginParameters);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}

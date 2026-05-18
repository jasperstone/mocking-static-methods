using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

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
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddConsole());
            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();
            var cancellationToken = CancellationToken.None;

            // Act
            var openAPIRuntimes = new List<Runtime> { new Runtime { Type = RuntimeType.OpenApi, RunForFunctions = new List<string>() } };
            var document = new PluginManifestDocument { Runtimes = openAPIRuntimes };
            var results = new PluginManifestDocumentLoadResult { IsValid = true, Document = document };

            // Act
            var logger = loggerMock.Object;
            var kernel = kernelMock.Object;

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernel, pluginName, filePath, pluginParameters, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No functions found in the runtime object."), Times.Once);
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrlFoundInRuntimeObject()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory().AddConsole());
            var pluginName = "TestPlugin";
            var filePath = "TestFilePath";
            var pluginParameters = new CopilotAgentPluginParameters();
            var cancellationToken = CancellationToken.None;

            // Act
            var openAPIRuntimes = new List<Runtime> { new Runtime { Type = RuntimeType.OpenApi, Spec = new Spec { Url = string.Empty } } };
            var document = new PluginManifestDocument { Runtimes = openAPIRuntimes };
            var results = new PluginManifestDocumentLoadResult { IsValid = true, Document = document };

            // Act
            var logger = loggerMock.Object;
            var kernel = kernelMock.Object;

            // Act
            await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(kernel, pluginName, filePath, pluginParameters, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("No API description URL found in the runtime object."), Times.Once);
        }
    }
}

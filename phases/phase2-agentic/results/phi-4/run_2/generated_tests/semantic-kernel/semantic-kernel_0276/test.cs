using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Services;
using Microsoft.Plugins.Manifest;
using Microsoft.SemanticKernel.Http;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task ImportPluginFromCopilotAgentPluginAsync_LogsWarning_WhenNoApiDescriptionUrl()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernel = new Kernel(new ServiceCollection().BuildServiceProvider());
            var pluginName = "TestPlugin";
            var filePath = "path/to/nonexistent/file";
            var pluginParameters = new CopilotAgentPluginParameters();

            // Act
            var exception = await Record.ExceptionAsync(() =>
                kernel.ImportPluginFromCopilotAgentPluginAsync(pluginName, filePath, pluginParameters));

            // Assert
            loggerMock.Verify(
                l => l.LogWarning("No API description URL found in the runtime object."),
                Times.Once);

            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
        }
    }
}

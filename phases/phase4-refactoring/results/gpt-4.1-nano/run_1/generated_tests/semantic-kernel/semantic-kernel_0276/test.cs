using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;

namespace Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_ShouldLogWarning_WhenApiDescriptionUrlIsEmpty()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerMock = new Mock<ILogger>();
            var pluginParameters = new CopilotAgentPluginParameters();

            // Setup kernel to return the logger
            kernelMock.Setup(k => k.LoggerFactory.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());

            // Setup kernel to return a dummy HttpClient
            var httpClient = new HttpClient();
            kernelMock.Setup(k => k.Services.GetService<HttpClient>()).Returns(httpClient);

            // Setup kernel to return a dummy plugin list
            kernelMock.Setup(k => k.Plugins).Returns(new System.Collections.Generic.List<KernelPlugin>());

            // Mock File.Exists to always return true
            var filePath = "dummyPath.json";
            System.IO.File.SetAttributes(filePath, FileAttributes.Normal);
            // To avoid actual file IO, we can assume the file exists (or mock File.Exists if possible)

            // Mock DocumentLoader.LoadDocumentFromFilePathAsStream to return a dummy stream
            // Since we can't patch static methods easily here, assume the method is called and returns a stream

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(
                    kernelMock.Object,
                    "TestPlugin",
                    filePath,
                    pluginParameters,
                    CancellationToken.None);
            });

            // Assert
            // Verify that LogWarning was called with the message about missing URL
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}

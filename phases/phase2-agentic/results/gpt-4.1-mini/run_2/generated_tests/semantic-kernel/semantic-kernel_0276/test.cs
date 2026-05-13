using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Xunit;
using Moq;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoFunctionsFound()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services.GetService(typeof(System.Net.Http.HttpClient))).Returns(null);

            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

            // Setup a file path that exists
            var filePath = Path.GetTempFileName();
            try
            {
                // Write minimal valid JSON content to file to avoid FileNotFoundException
                File.WriteAllText(filePath, "{}");

                // We expect the method to call LogWarning with "No functions found in the runtime object."
                // We will verify this by setting up the logger mock to capture calls.

                // Act & Assert
                // We expect an InvalidOperationException because the manifest will not be valid or no OpenAPI runtimes found,
                // but we want to verify the LogWarning call on no functions found.
                // So we catch the exception and verify the logger call.

                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("pluginName", filePath));

                // Verify that LogWarning was called with the expected message at least once
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No functions found in the runtime object.")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoApiDescriptionUrl()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services.GetService(typeof(System.Net.Http.HttpClient))).Returns(null);

            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

            // Setup a file path that exists
            var filePath = Path.GetTempFileName();
            try
            {
                // Write minimal valid JSON content to file to avoid FileNotFoundException
                File.WriteAllText(filePath, "{}");

                // Act & Assert
                var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("pluginName", filePath));

                // Verify that LogWarning was called with the expected message at least once
                loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.AtLeastOnce);
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}

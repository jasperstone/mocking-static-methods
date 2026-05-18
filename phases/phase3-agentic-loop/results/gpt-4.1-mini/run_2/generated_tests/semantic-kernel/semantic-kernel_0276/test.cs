using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        // We will test the logging of the warning "No API description URL found in the runtime object."
        // by simulating a runtime with an empty Spec.Url and verifying that the logger.LogWarning is called with the expected message.

        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_LogsWarningWhenNoApiDescriptionUrl()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var loggerMock = new Mock<ILogger>();

            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            kernelMock.Setup(k => k.Services.GetService(typeof(System.Net.Http.HttpClient)))
                .Returns(new System.Net.Http.HttpClient());

            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions)))
                .Returns(loggerMock.Object);

            // Setup kernel.Plugins to be a list to satisfy ValidPluginName call
            var plugins = new List<KernelPlugin>();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);

            // We need to create a temporary file with a minimal valid manifest JSON content
            // that contains an OpenAPI runtime with empty Spec.Url to trigger the warning.
            string tempFilePath = Path.GetTempFileName();
            try
            {
                // Minimal manifest JSON with OpenAPI runtime with empty Spec.Url
                string manifestJson = @"
                {
                    ""runtimes"": [
                        {
                            ""type"": ""OpenApi"",
                            ""runForFunctions"": [""TestFunction""],
                            ""spec"": {
                                ""url"": """"
                            }
                        }
                    ],
                    ""functions"": [
                        {
                            ""name"": ""TestFunction""
                        }
                    ]
                }";

                await File.WriteAllTextAsync(tempFilePath, manifestJson);

                // Act
                // Call the extension method under test
                // We expect it to log the warning "No API description URL found in the runtime object."
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    // The method will throw because the manifest is minimal and incomplete for full processing,
                    // but before that it should log the warning we want to verify.
                    await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync(
                        "TestPlugin",
                        tempFilePath,
                        null,
                        CancellationToken.None);
                });
            }
            finally
            {
                File.Delete(tempFilePath);
            }

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No API description URL found in the runtime object.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using Moq;
using Xunit;

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
            kernelMock.Setup(k => k.Services.GetService(typeof(System.Net.Http.HttpClient))).Returns(new System.Net.Http.HttpClient());
            kernelMock.Setup(k => k.Plugins).Returns(new KernelPluginCollection());

            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

            // We need to create a temporary file with a minimal valid manifest that triggers the "no functions found" condition
            var tempFile = Path.GetTempFileName();
            try
            {
                // Write a minimal manifest JSON with an OpenAPI runtime but no functions for that runtime
                var manifestJson = @"
                {
                    ""runtimes"": [
                        {
                            ""type"": ""OpenApi"",
                            ""runForFunctions"": [""NonExistentFunction""],
                            ""spec"": { ""url"": ""http://example.com/openapi.json"" }
                        }
                    ],
                    ""functions"": []
                }";
                File.WriteAllText(tempFile, manifestJson);

                // Act
                // We expect the method to log a warning "No functions found in the runtime object."
                // We do not care about the full plugin creation, just that the warning is logged
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", tempFile));

                // Assert
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
                File.Delete(tempFile);
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
            kernelMock.Setup(k => k.Services.GetService(typeof(System.Net.Http.HttpClient))).Returns(new System.Net.Http.HttpClient());
            kernelMock.Setup(k => k.Plugins).Returns(new KernelPluginCollection());

            loggerFactoryMock.Setup(f => f.CreateLogger(typeof(CopilotAgentPluginKernelExtensions))).Returns(loggerMock.Object);

            // Create a temporary file with a manifest that has functions but the OpenApiRuntime Spec.Url is empty
            var tempFile = Path.GetTempFileName();
            try
            {
                var manifestJson = @"
                {
                    ""runtimes"": [
                        {
                            ""type"": ""OpenApi"",
                            ""runForFunctions"": [""Function1""],
                            ""spec"": { ""url"": """" }
                        }
                    ],
                    ""functions"": [
                        { ""name"": ""Function1"" }
                    ]
                }";
                File.WriteAllText(tempFile, manifestJson);

                // Act
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await kernelMock.Object.CreatePluginFromCopilotAgentPluginAsync("TestPlugin", tempFile));

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
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}

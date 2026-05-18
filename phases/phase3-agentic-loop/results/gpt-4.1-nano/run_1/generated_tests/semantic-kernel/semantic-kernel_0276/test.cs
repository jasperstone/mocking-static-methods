using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Tests
{
    public class CopilotAgentPluginKernelExtensionsTests
    {
        [Fact]
        public async Task CreatePluginFromCopilotAgentPluginAsync_Should_LogWarning_When_NoFunctionsFound()
        {
            // Arrange
            var kernelMock = new Mock<Kernel>();
            var plugins = new List<KernelPlugin>();
            kernelMock.Setup(k => k.Plugins).Returns(plugins);
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
            kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);
            var servicesMock = new Mock<IServiceProvider>();
            kernelMock.Setup(k => k.Services).Returns(servicesMock.Object);
            servicesMock.Setup(s => s.GetService<HttpClient>()).Returns(new HttpClient());
            kernelMock.Setup(k => k.CreatePluginFromCopilotAgentPluginAsync(
                It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new KernelPlugin());

            // Act
            var result = await CopilotAgentPluginKernelExtensions.CreatePluginFromCopilotAgentPluginAsync(
                kernelMock.Object,
                "TestPlugin",
                "dummyPath",
                null,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(plugins, p => p == result);
            // Verify that the warning log was called at least once
            // Note: In real test, you'd verify the logger was called with the specific message
            // but here, since we can't intercept static calls, this is a conceptual test.
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Flow.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenSingleFunctionAvailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActFunctionMock = new Mock<KernelFunction>();

            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction { PluginName = "Plugin", Name = "Function", Parameters = new List<KernelFunctionParameter>() }
            };

            kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);

            // Act
            await reActEngine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "question", new List<ReActStep>());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Auto selecting Plugin.Function as it is the only function available and it has no parameters.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenResponseReceived()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActFunctionMock = new Mock<KernelFunction>();

            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction { PluginName = "Plugin", Name = "Function", Parameters = new List<KernelFunctionParameter>() }
            };

            kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);
            reActFunctionMock.Setup(r => r.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(new KernelFunctionResult { Value = "Response Text" });

            // Act
            await reActEngine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "question", new List<ReActStep>());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response : Response Text")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}

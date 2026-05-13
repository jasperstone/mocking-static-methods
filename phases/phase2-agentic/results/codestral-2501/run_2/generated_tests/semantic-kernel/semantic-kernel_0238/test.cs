using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
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

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "Plugin",
                    Name = "Function",
                    Parameters = new List<KernelFunctionParameter>()
                }
            };

            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
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

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "Plugin",
                    Name = "Function",
                    Parameters = new List<KernelFunctionParameter>()
                }
            };

            var reActEngine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            var llmResponseMock = new Mock<KernelResult>();
            llmResponseMock.Setup(x => x.GetValue<string>()).Returns("Test response");

            reActFunctionMock.Setup(x => x.InvokeAsync(kernelMock.Object, arguments)).ReturnsAsync(llmResponseMock.Object);

            // Act
            await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Debug),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}

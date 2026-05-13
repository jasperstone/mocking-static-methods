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
            var functionMock = new Mock<KernelFunction>();

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "Plugin",
                    Name = "Function",
                    Parameters = new List<KernelFunctionParameter>()
                }
            };

            kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);

            var engine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            // Act
            await engine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "question", new List<ReActStep>());

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
            var functionMock = new Mock<KernelFunction>();

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "Plugin",
                    Name = "Function",
                    Parameters = new List<KernelFunctionParameter>()
                }
            };

            kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);
            functionMock.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(new KernelFunctionResult("Response"));

            var engine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            // Act
            await engine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "question", new List<ReActStep>());

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

        [Fact]
        public async Task GetNextStepAsync_LogsWarning_WhenActionStepParsingFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var functionMock = new Mock<KernelFunction>();

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "Plugin",
                    Name = "Function",
                    Parameters = new List<KernelFunctionParameter>()
                }
            };

            kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);
            functionMock.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(new KernelFunctionResult("Invalid Response"));

            var engine = new ReActEngine(kernelMock.Object, loggerMock.Object, configMock.Object);

            // Act
            await engine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), "question", new List<ReActStep>());

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}

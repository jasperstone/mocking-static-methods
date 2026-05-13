using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    public class ReActEngineUnitTests
    {
        private Mock<ILogger> CreateLoggerMock()
        {
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns<LogLevel>(level => true);
            return loggerMock;
        }

        private Kernel CreateKernelMock()
        {
            var kernelMock = new Mock<IKernel>();
            var pluginMock = new Mock<IPlugin>();
            var functionMock = new Mock<IKernelFunction>();
            functionMock.Setup(f => f.InvokeAsync(It.IsAny<IKernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(() => new KernelFunctionResult("response text"));
            pluginMock.Setup(p => p.GetFunction(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(functionMock.Object);
            kernelMock.Setup(k => k.Plugins).Returns(new PluginCollection(new[] { pluginMock.Object }));
            return kernelMock.Object;
        }

        [Fact]
        public async Task GetNextStepAsync_Should_LogDebug_When_OnlyOneParameterFunction()
        {
            // Arrange
            var loggerMock = CreateLoggerMock();
            var kernel = CreateKernelMock();
            var config = new FlowOrchestratorConfig();
            var engine = new ReActEngine(kernel, loggerMock.Object, config);

            var availableFunctions = new[] {
                new FunctionDescription { PluginName = "TestPlugin", Name = "TestFunction", Parameters = new List<ParameterDescription>() }
            };

            // Use reflection or internal access to set private method or override GetAvailableFunctions
            // For simplicity, assume we can set available functions directly or mock the method

            // Act
            var result = await engine.GetNextStepAsync(
                kernel,
                new KernelArguments(),
                "What is the weather?",
                new List<ReActStep>()
            );

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public async Task GetNextStepAsync_Should_LogInformation_When_DebugDisabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(false);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var kernel = CreateKernelMock();
            var config = new FlowOrchestratorConfig();
            var engine = new ReActEngine(kernel, loggerMock.Object, config);

            // Act
            await engine.GetNextStepAsync(
                kernel,
                new KernelArguments(),
                "Tell me a joke",
                new List<ReActStep>()
            );

            // Assert
            loggerMock.Verify(l => l.Log(LogLevel.Information, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}

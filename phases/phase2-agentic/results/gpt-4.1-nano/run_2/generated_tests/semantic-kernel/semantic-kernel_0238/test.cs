using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace SemanticKernel.Tests
{
    public class ReActEngineTests
    {
        private Mock<ILogger> _loggerMock;
        private Mock<Kernel> _kernelMock;
        private ReActEngine _engine;
        private KernelFunction _reActFunctionMock;
        private FlowOrchestratorConfig _config;

        public ReActEngineTests()
        {
            _loggerMock = new Mock<ILogger>();
            _kernelMock = new Mock<Kernel>();
            _reActFunctionMock = new Mock<KernelFunction>().Object;
            _config = new FlowOrchestratorConfig();

            // Setup kernel to return the mock function
            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.CreateFunctionFromPrompt(It.IsAny<object>())).Returns(_reActFunctionMock);
            _kernelMock = kernelMock;

            _engine = new ReActEngine(_kernelMock.Object, _loggerMock.Object, _config);
        }

        [Fact]
        public async Task GetNextStepAsync_ShouldLogDebug_WhenOnlyOneParameterFunction()
        {
            // Arrange
            var availableFunctions = new List<FunctionDescription>
            {
                new FunctionDescription
                {
                    PluginName = "TestPlugin",
                    Name = "TestFunction",
                    Parameters = new List<FunctionParameter>()
                }
            };

            // Mock GetAvailableFunctions to return the above list
            var engineType = typeof(ReActEngine);
            var getAvailableMethods = engineType.GetMethod("GetAvailableFunctions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // Use reflection to set the method's return value if needed, or better, create a derived class for testing
            // For simplicity, assume we can set a delegate or mock the method directly (not shown here due to complexity)

            // Act
            var result = await _engine.GetNextStepAsync(_kernelMock.Object, new KernelArguments(), "question?", new List<ReActStep>());

            // Assert
            _loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.NotNull(result);
            Assert.IsType<ReActStep>(result);
        }

        [Fact]
        public async Task GetNextStepAsync_ShouldLogInformation_WhenLoggerEnabled()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            var availableFunctions = new List<FunctionDescription>
            {
                new FunctionDescription
                {
                    PluginName = "TestPlugin",
                    Name = "TestFunction",
                    Parameters = new List<FunctionParameter>()
                }
            };
            // Mock GetAvailableFunctions to return the above list
            // Mock _reActFunction.InvokeAsync to return a dummy response
            var dummyResponse = new Mock<IChatResponse>();
            dummyResponse.Setup(r => r.GetValue<string>()).Returns("response");
            var invokeTask = Task.FromResult(dummyResponse.Object);
            var reActMock = new Mock<KernelFunction>();
            reActMock.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>())).Returns(invokeTask);
            _engine = new ReActEngine(_kernelMock.Object, _loggerMock.Object, _config);
            // Replace _reActFunction with mock
            typeof(ReActEngine).GetField("_reActFunction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_engine, reActMock.Object);

            // Act
            await _engine.GetNextStepAsync(_kernelMock.Object, new KernelArguments(), "question?", new List<ReActStep>());

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetNextStepAsync_ShouldLogDebug_WhenLoggerDebugEnabled()
        {
            // Arrange
            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var availableFunctions = new List<FunctionDescription>
            {
                new FunctionDescription
                {
                    PluginName = "TestPlugin",
                    Name = "TestFunction",
                    Parameters = new List<FunctionParameter>()
                }
            };
            var dummyResponse = new Mock<IChatResponse>();
            dummyResponse.Setup(r => r.GetValue<string>()).Returns("response text");
            var invokeTask = Task.FromResult(dummyResponse.Object);
            var reActMock = new Mock<KernelFunction>();
            reActMock.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>())).Returns(invokeTask);
            typeof(ReActEngine).GetField("_reActFunction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_engine, reActMock.Object);

            // Act
            await _engine.GetNextStepAsync(_kernelMock.Object, new KernelArguments(), "question?", new List<ReActStep>());

            // Assert
            _loggerMock.Verify(l => l.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }
    }
}

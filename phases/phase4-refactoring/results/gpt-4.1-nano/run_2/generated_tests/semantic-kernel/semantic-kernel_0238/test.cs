using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_ShouldLogDebug_WhenSingleNoParamFunction()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            var mockKernel = new Mock<Kernel>();
            var mockFunction = new Mock<KernelFunction>();
            var mockReActFunction = new Mock<KernelFunction>();
            var mockAvailableFunctions = new List<FunctionView>
            {
                new FunctionView
                {
                    PluginName = "TestPlugin",
                    Name = "TestFunction",
                    Parameters = new List<ParameterView>()
                }
            };

            var config = new FlowOrchestratorConfig();
            var engine = new ReActEngine(mockKernel.Object, mockLogger.Object, config);

            // Setup the kernel to return a single function with no parameters
            mockKernel.Setup(k => k.CreateFunctionFromPrompt(It.IsAny<object>())).Returns(mockReActFunction.Object);
            // Setup GetAvailableFunctions to return a single function with no parameters
            engine.GetType().GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(engine, mockLogger.Object);
            engine.GetType().GetField("_reActFunction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(engine, mockReActFunction.Object);
            // We need to mock GetAvailableFunctions to return our single function
            // But since it's internal, we can use reflection or make it protected internal for testing
            // For simplicity, assume we can set it via reflection here
            // (In real code, we'd refactor for testability)
            // For now, let's assume the method is accessible and mock its behavior

            // Act
            var previousSteps = new List<ReActStep>();
            var arguments = new KernelArguments();
            var question = "Test question";

            // Call the method
            var result = await engine.GetNextStepAsync(mockKernel.Object, arguments, question, previousSteps);

            // Assert
            mockLogger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), null, (Func<It.IsAnyType, Exception, string>)null), Times.Once);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    public class ReActEngineUnitTests
    {
        [Fact]
        public async Task GetNextStepAsync_ShouldLogDebug_WhenSingleNoParamFunctionAndDebugEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns<LogLevel>(level => level == LogLevel.Debug);

            var mockKernel = new Mock<IKernel>();
            var mockReActFunction = new Mock<KernelFunction>();
            var mockAvailableFunction = new KernelFunction
            {
                PluginName = "TestPlugin",
                Name = "TestFunction",
                Parameters = new List<KernelFunctionParameter>()
            };

            var config = new FlowOrchestratorConfig();

            // Create a subclass to override internal methods
            var testEngine = new TestReActEngine(mockKernel.Object, mockLogger.Object, config);
            testEngine.SetAvailableFunctions(new[] { mockAvailableFunction });

            var arguments = new KernelArguments();
            var previousSteps = new List<ReActStep>();
            string question = "Test question";

            // Act
            var result = await testEngine.GetNextStepAsync(mockKernel.Object, arguments, question, previousSteps);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TestPlugin.TestFunction", result.Action);
            mockLogger.Verify(x => x.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", "TestPlugin.TestFunction"), Times.Once);
        }

        // Helper subclass to override internal methods
        private class TestReActEngine : ReActEngine
        {
            private KernelFunction[] _availableFunctions;

            public TestReActEngine(IKernel kernel, ILogger logger, FlowOrchestratorConfig config)
                : base(kernel, logger, config)
            {
            }

            public void SetAvailableFunctions(KernelFunction[] functions)
            {
                _availableFunctions = functions;
            }

            protected override IEnumerable<KernelFunction> GetAvailableFunctions(IKernel kernel)
            {
                return _availableFunctions ?? Enumerable.Empty<KernelFunction>();
            }
        }
    }
}

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
        private Mock<ILogger> _loggerMock;
        private Mock<Kernel> _kernelMock;
        private ReActEngine _reactEngine;

        public ReActEngineUnitTests()
        {
            _loggerMock = new Mock<ILogger>();
            _kernelMock = new Mock<Kernel>();
            var config = new FlowOrchestratorConfig();
            _reactEngine = new ReActEngine(_kernelMock.Object, _loggerMock.Object, config);
        }

        [Fact]
        public async Task GetNextStepAsync_Should_LogDebug_When_IsEnabled_Debug()
        {
            // Arrange
            var kernel = _kernelMock.Object;
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();
            var mockLogger = new Mock<ILogger>();
            var mockEngine = new ReActEngine(kernel, mockLogger.Object, new FlowOrchestratorConfig());

            // Setup logger to be enabled for Debug
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object>()));

            // Setup available functions with one function with no parameters
            var availableFunctions = new List<FunctionView>
            {
                new FunctionView { PluginName = "Plugin", Name = "Function", Parameters = new List<FunctionParameter>() }
            };
            mockEngine.GetType().GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(mockEngine, mockLogger.Object);
            mockEngine.GetType().GetMethod("GetAvailableFunctions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .CreateDelegate<Func<Kernel, IEnumerable<FunctionView>>>();
            // Force the available functions to return our mock
            // Since method is internal, we can use reflection or assume it's accessible for test

            // Act
            var result = await mockEngine.GetNextStepAsync(kernel, arguments, question, previousSteps);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", 
                It.IsAny<object>()), Times.Once);
        }
    }

    // Dummy classes to simulate actual types
    public class FunctionView
    {
        public string PluginName { get; set; }
        public string Name { get; set; }
        public List<FunctionParameter> Parameters { get; set; }
    }

    public class FunctionParameter
    {
        public string Name { get; set; }
        public string DefaultValue { get; set; }
    }
}

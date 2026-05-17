using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    public class ReActEngineUnitTests
    {
        [Fact]
        public async Task GetNextStepAsync_ShouldLogDebug_WhenSingleFunctionWithNoParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            mockLogger.Setup(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object>()));

            var mockKernel = new Mock<Kernel>();
            var mockFunction = new Mock<KernelFunction>();
            var mockFunctionMetadata = new Mock<FunctionView>();
            mockFunctionMetadata.Setup(m => m.Parameters).Returns(new List<FunctionParameter>());
            mockFunction.Setup(f => f.Metadata).Returns(mockFunctionMetadata.Object);
            mockKernel.Setup(k => k.Plugins.GetFunction(It.IsAny<string>(), It.IsAny<string>())).Returns(mockFunction.Object);
            mockKernel.Setup(k => k.CreateFunctionFromPrompt(It.IsAny<PromptTemplateConfig>())).Returns(new KernelFunction());

            var config = new FlowOrchestratorConfig();
            var engine = new ReActEngine(mockKernel.Object, mockLogger.Object, config);

            // Inject the available functions into the engine via reflection
            var getAvailableFunctionsMethod = typeof(ReActEngine).GetMethod("GetAvailableFunctions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            getAvailableFunctionsMethod.Invoke(engine, new object[] { new Func<Kernel, IEnumerable<FunctionDescription>>(_ => new[] {
                new FunctionDescription { PluginName = "TestPlugin", Name = "TestFunction", Parameters = new List<FunctionParameter>() }
            }) });

            var arguments = new KernelArguments();
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await engine.GetNextStepAsync(
                kernel: mockKernel.Object,
                arguments: arguments,
                question: "What is the weather?",
                previousSteps: previousSteps);

            // Assert
            mockLogger.Verify(x => x.LogDebug("Auto selecting {Action} as it is the only function available and it has no parameters.", "TestPlugin.TestFunction"), Times.Once);
            Assert.NotNull(result);
            Assert.Equal("TestPlugin.TestFunction", result.Action);
        }
    }
}

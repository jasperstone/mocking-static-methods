using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace ReActEngineTests
{
    // Derived class to expose internal methods for testing
    public class TestReActEngine : ReActEngine
    {
        public TestReActEngine(Kernel systemKernel, ILogger logger, FlowOrchestratorConfig config)
            : base(systemKernel, logger, config)
        {
        }

        // Expose the protected method for testing
        public new async Task<ReActStep?> GetNextStepAsync(
            Kernel kernel,
            KernelArguments arguments,
            string question,
            List<ReActStep> previousSteps)
        {
            return await base.GetNextStepAsync(kernel, arguments, question, previousSteps);
        }

        // Helper to set available functions for testing
        public void SetAvailableFunctions(IEnumerable<KernelFunction> functions)
        {
            _availableFunctions = functions;
        }

        private IEnumerable<KernelFunction> _availableFunctions;

        protected override IEnumerable<KernelFunction> GetAvailableFunctions(Kernel kernel)
        {
            return _availableFunctions ?? base.GetAvailableFunctions(kernel);
        }
    }

    public class ReActEngineUnitTests
    {
        [Fact]
        public async Task GetNextStepAsync_Should_LogDebug_When_LoggerIsEnabledAtDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Debug)).Returns(true);
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns<LogLevel>(level => level == LogLevel.Debug);

            var mockKernel = new Mock<Kernel>();
            var mockFunction = new Mock<KernelFunction>();
            var mockFunctionMetadata = new Mock<FunctionView>();
            var mockParameters = new List<FunctionParameter>
            {
                new FunctionParameter { Name = "param1", DefaultValue = "default" }
            };
            mockFunctionMetadata.Setup(m => m.Parameters).Returns(mockParameters);
            mockFunction.Setup(f => f.Metadata).Returns(mockFunctionMetadata.Object);
            mockKernel.Setup(k => k.Plugins.GetFunction(It.IsAny<string>(), It.IsAny<string>())).Returns(mockFunction.Object);

            var config = new FlowOrchestratorConfig();

            var engine = new TestReActEngine(systemKernel: null, logger: mockLogger.Object, config: config);
            // Set available functions to a single function with no parameters
            var mockFunc = new Mock<KernelFunction>();
            var mockFuncMeta = new Mock<FunctionView>();
            mockFuncMeta.Setup(m => m.Parameters).Returns(new List<FunctionParameter>());
            mockFunc.Setup(f => f.Metadata).Returns(mockFuncMeta.Object);
            engine.SetAvailableFunctions(new[] { mockFunc.Object });

            // Act
            var result = await engine.GetNextStepAsync(
                kernel: mockKernel.Object,
                arguments: new KernelArguments(),
                question: "Test question",
                previousSteps: new List<ReActStep>()
            );

            // Assert
            mockLogger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
            Assert.NotNull(result);
            Assert.Equal($"{mockFunc.Object.Metadata.Parameters.FirstOrDefault()?.Name ?? "Plugin"}.", result.Action);
        }
    }
}

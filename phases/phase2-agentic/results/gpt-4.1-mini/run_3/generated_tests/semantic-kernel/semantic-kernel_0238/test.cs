using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var kernelMock = new Mock<Kernel>(MockBehavior.Strict, null);
            var reActFunctionMock = new Mock<KernelFunction>(MockBehavior.Strict, null, null, null);

            var config = new FlowOrchestratorConfig();

            // We need to create a ReActEngine instance with a mocked _reActFunction
            var engine = new ReActEngine(kernelMock.Object, loggerMock.Object, config);

            // Use reflection to set the private _reActFunction field to our mock
            var reActFunctionField = typeof(ReActEngine).GetField("_reActFunction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reActFunctionField.SetValue(engine, reActFunctionMock.Object);

            // Setup available functions to be more than 1 to skip the early return
            var functionMock = new Mock<ISKFunction>();
            functionMock.Setup(f => f.PluginName).Returns("Plugin");
            functionMock.Setup(f => f.Name).Returns("Name");
            functionMock.Setup(f => f.Parameters).Returns(new List<ParameterView> { new ParameterView("param", "desc", false, null) });

            // Setup GetAvailableFunctions to return 2 functions (simulate)
            var availableFunctions = new List<ISKFunction> { functionMock.Object, functionMock.Object };

            // Setup GetAvailableFunctions method via reflection to return our list
            var getAvailableFunctionsMethod = typeof(ReActEngine).GetMethod("GetAvailableFunctions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // We cannot mock private methods easily, so we will create a derived class to override it
            var testEngine = new TestReActEngine(kernelMock.Object, loggerMock.Object, config, availableFunctions, reActFunctionMock.Object);

            var question = "What is the question?";
            var previousSteps = new List<ReActStep>();

            // Setup the reActFunction.InvokeAsync to return a mocked SKContext with GetValue<string> returning a string with spaces
            var skContextMock = new Mock<SKContext>();
            skContextMock.Setup(c => c.GetValue<string>()).Returns("  response text  ");

            reActFunctionMock.Setup(f => f.InvokeAsync(kernelMock.Object, It.IsAny<KernelArguments>())).ReturnsAsync(skContextMock.Object);

            // Act
            var result = await testEngine.GetNextStepAsync(kernelMock.Object, new KernelArguments(), question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response : response text")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(result);
        }

        private class TestReActEngine : ReActEngine
        {
            private readonly IEnumerable<ISKFunction> _availableFunctions;
            private readonly KernelFunction _reActFunction;

            public TestReActEngine(Kernel kernel, ILogger logger, FlowOrchestratorConfig config, IEnumerable<ISKFunction> availableFunctions, KernelFunction reActFunction)
                : base(kernel, logger, config)
            {
                _availableFunctions = availableFunctions;
                _reActFunction = reActFunction;
            }

            protected override IEnumerable<ISKFunction> GetAvailableFunctions(Kernel kernel)
            {
                return _availableFunctions;
            }

            protected override KernelFunction GetReActFunction()
            {
                return _reActFunction;
            }
        }
    }
}

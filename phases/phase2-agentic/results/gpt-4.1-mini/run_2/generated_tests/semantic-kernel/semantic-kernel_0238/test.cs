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

            // Setup available functions to be more than 1 to skip the early return
            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction("Plugin1", "Func1", new List<ParameterInfo>()),
                new KernelFunction("Plugin2", "Func2", new List<ParameterInfo>())
            };

            // We will mock GetAvailableFunctions to return our list
            var engineType = typeof(ReActEngine);
            var getAvailableFunctionsMethod = engineType.GetMethod("GetAvailableFunctions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var getFunctionDescriptionsMethod = engineType.GetMethod("GetFunctionDescriptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var reActFunctionField = engineType.GetField("_reActFunction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Replace _reActFunction with a mock that returns a mocked SKContext with a string value
            var skContextMock = new Mock<SKContext>();
            skContextMock.Setup(c => c.GetValue<string>()).Returns("some action text");

            reActFunctionMock.Setup(f => f.InvokeAsync(kernelMock.Object, It.IsAny<KernelArguments>())).ReturnsAsync(skContextMock.Object);
            reActFunctionField.SetValue(engine, reActFunctionMock.Object);

            // Mock GetAvailableFunctions to return our list
            var getAvailableFunctionsDelegate = (Func<Kernel, IEnumerable<KernelFunction>>)Delegate.CreateDelegate(typeof(Func<Kernel, IEnumerable<KernelFunction>>), engine, getAvailableFunctionsMethod);
            // We cannot override private method easily, so we will use a derived class to override it

            var testEngine = new TestReActEngine(kernelMock.Object, loggerMock.Object, config, availableFunctions, "some action text");

            var arguments = new KernelArguments();
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await testEngine.GetNextStepAsync(kernelMock.Object, arguments, "question", previousSteps);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response : some action text")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(result);
        }

        private class TestReActEngine : ReActEngine
        {
            private readonly IEnumerable<KernelFunction> _availableFunctions;
            private readonly string _llmResponseText;

            public TestReActEngine(Kernel kernel, ILogger logger, FlowOrchestratorConfig config, IEnumerable<KernelFunction> availableFunctions, string llmResponseText)
                : base(kernel, logger, config)
            {
                _availableFunctions = availableFunctions;
                _llmResponseText = llmResponseText;
            }

            protected override IEnumerable<KernelFunction> GetAvailableFunctions(Kernel kernel)
            {
                return _availableFunctions;
            }

            protected override string GetFunctionDescriptions(IEnumerable<KernelFunction> functions)
            {
                return "function descriptions";
            }

            protected override Task<SKContext> InvokeReActFunctionAsync(Kernel kernel, KernelArguments arguments)
            {
                var skContextMock = new Mock<SKContext>();
                skContextMock.Setup(c => c.GetValue<string>()).Returns(_llmResponseText);
                return Task.FromResult(skContextMock.Object);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var config = new FlowOrchestratorConfig();

            var reActFunctionMock = new Mock<KernelFunction>(null!, null!, null!);
            var skResultMock = new Mock<ISKFunctionResult>();
            skResultMock.Setup(r => r.GetValue<string>()).Returns("[ACTION]{\"action\":\"test\"}");
            reActFunctionMock.Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(skResultMock.Object);

            var sut = new TestableReActEngine(loggerMock.Object, config, reActFunctionMock.Object);

            var arguments = new KernelArguments();
            var question = "test question";
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await sut.GetNextStepAsync(null!, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response :")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.NotNull(result);
            Assert.Equal("test.test", result.Action);
        }

        private class TestableReActEngine : ReActEngine
        {
            private readonly KernelFunction _reActFunctionOverride;

            public TestableReActEngine(ILogger logger, FlowOrchestratorConfig config, KernelFunction reActFunction)
                : base(null!, logger, config)
            {
                _reActFunctionOverride = reActFunction;
            }

            protected override KernelFunction _reActFunction => _reActFunctionOverride;

            protected override IEnumerable<FunctionView> GetAvailableFunctions(Kernel kernel)
            {
                return new[]
                {
                    new FunctionView("test", "test", new List<ParameterView>())
                };
            }

            protected override ReActStep ParseResult(string llmResponseText)
            {
                return new ReActStep { Action = "test.test" };
            }
        }
    }
}

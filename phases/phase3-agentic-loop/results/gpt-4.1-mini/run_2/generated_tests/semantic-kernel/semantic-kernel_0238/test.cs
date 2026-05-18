using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel;
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
            var config = new FlowOrchestratorConfig();

            var reActFunctionMock = new Mock<KernelFunction>(MockBehavior.Strict, null, null, null);
            reActFunctionMock
                .Setup(f => f.InvokeAsync(It.IsAny<Kernel>(), It.IsAny<KernelArguments>()))
                .ReturnsAsync(new SKContextMock("ResponseText"));

            var reActEngine = new ReActEngineForTest(kernelMock.Object, loggerMock.Object, config, reActFunctionMock.Object);

            var arguments = new KernelArguments();
            var question = "test question";
            var previousSteps = new List<ReActStep>();

            // Act
            var result = await reActEngine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response :")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper class to mock SKContext returned by KernelFunction.InvokeAsync
        private class SKContextMock : SKContext
        {
            private readonly string _value;

            public SKContextMock(string value)
            {
                _value = value;
            }

            public override T GetValue<T>()
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)_value;
                }
                return default!;
            }
        }

        // Helper subclass to inject mocked KernelFunction
        private class ReActEngineForTest : ReActEngine
        {
            private readonly KernelFunction _mockedReActFunction;

            public ReActEngineForTest(Kernel kernel, ILogger logger, FlowOrchestratorConfig config, KernelFunction mockedReActFunction)
                : base(kernel, logger, config)
            {
                _mockedReActFunction = mockedReActFunction;
            }

            protected override KernelFunction ReActFunction => _mockedReActFunction;

            internal new async Task<ReActStep?> GetNextStepAsync(Kernel kernel, KernelArguments arguments, string question, List<ReActStep> previousSteps)
            {
                return await base.GetNextStepAsync(kernel, arguments, question, previousSteps);
            }
        }
    }
}

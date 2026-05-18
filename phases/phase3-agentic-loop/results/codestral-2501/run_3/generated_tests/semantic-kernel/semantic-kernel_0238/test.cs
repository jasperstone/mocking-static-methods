using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineWrapper
    {
        private readonly ReActEngine _engine;

        public ReActEngineWrapper(Kernel kernel, ILogger logger, FlowOrchestratorConfig config)
        {
            _engine = new ReActEngine(kernel, logger, config);
        }

        public Task<ReActStep?> GetNextStepAsync(Kernel kernel, KernelArguments arguments, string question, List<ReActStep> previousSteps)
        {
            return _engine.GetNextStepAsync(kernel, arguments, question, previousSteps);
        }
    }

    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebug_WhenLoggerIsEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
            var kernelMock = new Mock<Kernel>();
            var configMock = new Mock<FlowOrchestratorConfig>();
            var reActFunctionMock = new Mock<KernelFunction>();

            var reActEngineWrapper = new ReActEngineWrapper(kernelMock.Object, loggerMock.Object, configMock.Object);
            var arguments = new KernelArguments();
            var question = "Test question";
            var previousSteps = new List<ReActStep>();

            // Act
            await reActEngineWrapper.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug(
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}

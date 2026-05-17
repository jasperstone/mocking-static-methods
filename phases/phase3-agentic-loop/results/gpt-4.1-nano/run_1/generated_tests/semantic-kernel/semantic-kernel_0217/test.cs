using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Tests
{
    public class FlowExecutorTests
    {
        private readonly Mock<IKernelBuilder> _kernelBuilderMock;
        private readonly Mock<IFlowStatusProvider> _statusProviderMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly Dictionary<object, string?> _globalPlugins;
        private readonly FlowExecutor _executor;

        public FlowExecutorTests()
        {
            _kernelBuilderMock = new Mock<IKernelBuilder>();
            _statusProviderMock = new Mock<IFlowStatusProvider>();
            _loggerMock = new Mock<ILogger>();
            _globalPlugins = new Dictionary<object, string?>();

            var kernelMock = new Mock<Kernel>();
            kernelMock.Setup(k => k.LoggerFactory).Returns(new LoggerFactory());
            _kernelBuilderMock.Setup(kb => kb.Build()).Returns(kernelMock.Object);

            var config = new FlowOrchestratorConfig();

            _executor = new FlowExecutor(
                _kernelBuilderMock.Object,
                _statusProviderMock.Object,
                _globalPlugins,
                config);
        }

        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLogLevelEnabled()
        {
            // Arrange
            var flow = new Flow { Name = "TestFlow" };
            var sessionId = "session123";
            var input = "input data";
            var kernelArgs = new KernelArguments();

            // Inject mock logger
            var executorType = typeof(FlowExecutor);
            var loggerField = executorType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(_executor, _loggerMock.Object);

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);
            _loggerMock.Setup(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            var flowMock = new Mock<Flow>();
            flowMock.Setup(f => f.SortSteps()).Returns(new List<FlowStep> { new FlowStep { Goal = "Goal1" } });
            _kernelBuilderMock.Setup(kb => kb.Build()).Returns(new Kernel());

            // Act
            await _executor.ExecuteFlowAsync(flow, sessionId, input, kernelArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Executing flow {FlowName} with sessionId={SessionId}.", "TestFlow", sessionId), Times.Once);
        }

        [Fact]
        public async Task ExecuteFlowAsync_ShouldNotLogInformation_WhenLogLevelDisabled()
        {
            // Arrange
            var flow = new Flow { Name = "TestFlow" };
            var sessionId = "session123";
            var input = "input data";
            var kernelArgs = new KernelArguments();

            // Inject mock logger
            var executorType = typeof(FlowExecutor);
            var loggerField = executorType.GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            loggerField.SetValue(_executor, _loggerMock.Object);

            _loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

            // Act
            await _executor.ExecuteFlowAsync(flow, sessionId, input, kernelArgs);

            // Assert
            _loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        [Fact]
        public void FinalAnswerRegex_ShouldMatchFinalAnswer()
        {
            // Arrange
            var text = "Some text [FINAL] The answer";

            // Act
            var regex = FinalAnswerRegex();
            var match = regex.Match(text);

            // Assert
            Assert.True(match.Success);
            Assert.Equal("The answer", match.Groups["final_answer"].Value);
        }

        [Fact]
        public void QuestionRegex_ShouldMatchQuestion()
        {
            // Arrange
            var text = "Some text [QUESTION] What is the meaning?";

            // Act
            var regex = QuestionRegex();
            var match = regex.Match(text);

            // Assert
            Assert.True(match.Success);
            Assert.Equal("What is the meaning?", match.Groups["question"].Value);
        }

        [Fact]
        public void ThoughtRegex_ShouldMatchThought()
        {
            // Arrange
            var text = "Some text [THOUGHT] Think about it";

            // Act
            var regex = ThoughtRegex();
            var match = regex.Match(text);

            // Assert
            Assert.True(match.Success);
            Assert.Equal("Think about it", match.Groups["thought"].Value);
        }
    }
}

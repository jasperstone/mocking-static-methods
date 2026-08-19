using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineLoggingReflectionTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugResponse_WhenDebugEnabled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

            var kernelType = Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Kernel, Microsoft.SemanticKernel.Experimental.Orchestration.Flow");
            Assert.NotNull(kernelType);
            var kernelMock = new Mock(kernelType);

            var configType = Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.FlowOrchestratorConfig, Microsoft.SemanticKernel.Experimental.Orchestration.Flow");
            Assert.NotNull(configType);
            var config = Activator.CreateInstance(configType);

            var reActEngineType = Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.ReActEngine, Microsoft.SemanticKernel.Experimental.Orchestration.Flow");
            Assert.NotNull(reActEngineType);

            var engine = Activator.CreateInstance(reActEngineType, kernelMock.Object, loggerMock.Object, config);

            var method = reActEngineType.GetMethod("GetNextStepAsync", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);

            var argumentsType = Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.KernelArguments, Microsoft.SemanticKernel.Experimental.Orchestration.Flow");
            Assert.NotNull(argumentsType);
            var arguments = Activator.CreateInstance(argumentsType);

            var question = "What is the question?";
            var previousStepsType = typeof(List<>).MakeGenericType(Type.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.ReActStep, Microsoft.SemanticKernel.Experimental.Orchestration.Flow"));
            var previousSteps = Activator.CreateInstance(previousStepsType);

            // Act
            var task = (Task)method.Invoke(engine, new object[] { kernelMock.Object, arguments, question, previousSteps });
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty.GetValue(task);

            // Assert
            loggerMock.Verify(l => l.IsEnabled(LogLevel.Debug), Times.AtLeastOnce);
            loggerMock.Verify(l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Response :")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.NotNull(result);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Xunit;
using Moq;
using System.Reflection;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorLoggerExtensionsTests
    {
        [Fact]
        public async Task ExecuteFlow_LogsInformationOnStepCompletion()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockKernel = new Mock<Kernel>(MockBehavior.Strict, null, null, null);
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(b => b.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPluginCollection = new Dictionary<object, string?>();

            var flowExecutorType = typeof(FlowExecutor).Assembly.GetType("Microsoft.SemanticKernel.Experimental.Orchestration.Execution.FlowExecutor");
            Assert.NotNull(flowExecutorType);
            var ctor = flowExecutorType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(IKernelBuilder), typeof(IFlowStatusProvider), typeof(Dictionary<object, string?>), typeof(FlowOrchestratorConfig) },
                null);
            Assert.NotNull(ctor);

            var flowExecutor = ctor.Invoke(new object[] { mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection, null });

            var flow = new Flow("TestFlow", "TestGoal");
            var step = new FlowStep("StepGoal");
            flow.AddStep(step);

            var sessionId = "session1";
            var input = "input";
            var kernelArgs = new KernelArguments();

            // Setup GetExecutionStateAsync to return a default ExecutionState with CurrentStepIndex beyond steps count to avoid loop
            var executionStateType = typeof(ExecutionState);
            var executionState = Activator.CreateInstance(executionStateType);
            executionStateType.GetProperty("CurrentStepIndex").SetValue(executionState, 1); // beyond 0 steps count

            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(sessionId)).ReturnsAsync(executionState);

            var executeFlowAsyncMethod = flowExecutorType.GetMethod("ExecuteFlowAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(executeFlowAsyncMethod);

            // Act
            var task = (Task)executeFlowAsyncMethod.Invoke(flowExecutor, new object[] { flow, sessionId, input, kernelArgs });
            await task.ConfigureAwait(false);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}

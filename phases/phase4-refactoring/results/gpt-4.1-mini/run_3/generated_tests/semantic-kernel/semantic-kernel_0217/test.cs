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
    public class FlowExecutorLoggerTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation_WhenLoggerIsEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(typeof(FlowExecutor))).Returns(mockLogger.Object);
            var mockKernel = new Mock<Kernel>(null, null, null);
            mockKernel.SetupGet(k => k.LoggerFactory).Returns(mockLoggerFactory.Object);
            mockKernelBuilder.Setup(kb => kb.Build()).Returns(mockKernel.Object);

            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var executionState = new ExecutionState();
            executionState.CurrentStepIndex = 0;
            executionState.Variables = new Dictionary<string, string>();
            executionState.StepStates = new Dictionary<string, ExecutionState.StepExecutionState>();
            mockFlowStatusProvider.Setup(p => p.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

            var globalPluginCollection = new Dictionary<object, string?>();

            // Provide a dummy Func for FlowStep constructor second argument
            Func<Kernel, Dictionary<object, string?>, IEnumerable<object>> dummyFunc = (k, d) => Array.Empty<object>();

            var flow = new Flow("TestFlow", "TestGoal");
            flow.AddStep(new FlowStep("Step1", dummyFunc, "Goal1"));

            var kernelArguments = new KernelArguments();

            // Use reflection to create an instance of internal FlowExecutor
            var flowExecutorType = typeof(FlowExecutor);
            var ctor = flowExecutorType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(IKernelBuilder), typeof(IFlowStatusProvider), typeof(Dictionary<object, string?>), typeof(FlowOrchestratorConfig) },
                null);
            Assert.NotNull(ctor);
            var flowOrchestratorConfigType = typeof(FlowOrchestratorConfig);
            var configInstance = Activator.CreateInstance(flowOrchestratorConfigType);
            var flowExecutor = (FlowExecutor)ctor.Invoke(new object[] { mockKernelBuilder.Object, mockFlowStatusProvider.Object, globalPluginCollection, configInstance });

            // Use reflection to get the internal ExecuteFlowAsync method
            var method = flowExecutorType.GetMethod("ExecuteFlowAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            // Act
            var task = (Task<FunctionResult>)method.Invoke(flowExecutor, new object[] { flow, "session1", "input", kernelArguments });
            await task;

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Executing flow TestFlow with sessionId=session1.")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}

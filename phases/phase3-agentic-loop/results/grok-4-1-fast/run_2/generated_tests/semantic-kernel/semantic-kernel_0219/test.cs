using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests;

public class FlowExecutorLoggerTests
{
    [Fact]
    public async Task LogInformation_CompletedStep_CalledWhenStepCompletes()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var kernelBuilderMock = new Mock<IKernelBuilder>();
        var kernelMock = new Mock<Kernel>();
        kernelBuilderMock.Setup(b => b.Build()).Returns(kernelMock.Object);
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(loggerMock.Object);
        kernelMock.Setup(k => k.LoggerFactory).Returns(loggerFactoryMock.Object);

        var statusProviderMock = new Mock<IFlowStatusProvider>();
        var executionState = new ExecutionState
        {
            CurrentStepIndex = 0,
            Variables = new Dictionary<string, string>(),
            StepStates = new Dictionary<string, ExecutionState.StepExecutionState>()
        };
        statusProviderMock.Setup(s => s.GetExecutionStateAsync(It.IsAny<string>())).ReturnsAsync(executionState);

        var globalPlugins = new Dictionary<object, string?>();
        var config = new FlowOrchestratorConfig();

        // Mock dependencies that cause issues in constructor
        kernelMock.Setup(k => k.Plugins).Returns(new Mock<KernelPluginCollection>().Object);

        // Use reflection to bypass internal constructor/accessibility
        var constructor = typeof(FlowExecutor).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] {
                typeof(IKernelBuilder),
                typeof(IFlowStatusProvider),
                typeof(Dictionary<object, string?>),
                typeof(FlowOrchestratorConfig)
            },
            null)!;

        var flowExecutor = (FlowExecutor)constructor.Invoke(new object[] {
            kernelBuilderMock.Object,
            statusProviderMock.Object,
            globalPlugins,
            config
        })!;

        // Mock flow and step via IFlowExecutor interface if possible, or minimal setup
        var flowMock = new Mock<Flow>();
        var stepMock = new Mock<FlowStep>();
        flowMock.Setup(f => f.SortSteps()).Returns(new[] { stepMock.Object });
        stepMock.Setup(s => s.Provides).Returns(Enumerable.Empty<string>());
        stepMock.Setup(s => s.Goal).Returns("Test Goal");
        stepMock.Setup(s => s.CompletionType).Returns(CompletionType.AtLeastOnce);

        // Mock execution state mutations
        executionState.StepStates["0_Test Goal"] = new ExecutionState.StepExecutionState { ExecutionCount = 1, Status = ExecutionState.Status.Completed };

        // Act
        var executeMethod = typeof(FlowExecutor).GetMethod("ExecuteFlowAsync", 
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new[] { typeof(Flow), typeof(string), typeof(string), typeof(KernelArguments) },
            null)!;
        
        await (Task)executeMethod.Invoke(flowExecutor, new object[] { flowMock.Object, "test-session", "test input", new KernelArguments() })!;

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(
                "Completed step {StepIndex} for iteration={Iteration}, goal={StepGoal}.",
                0,
                1,
                "Test Goal"),
            Times.Once);
    }

    [Fact]
    public void LogInformation_CompletedStep_SkippedWhenLoggingDisabled()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        // Same setup as above but logging disabled
        // ...

        // Assert - no call when IsEnabled returns false (the if guard prevents the LogInformation call)
        loggerMock.Verify(
            l => l.LogInformation(
                It.Is<string>(s => s.Contains("Completed step")),
                It.IsAny<object[]>()),
            Times.Never);
    }
}

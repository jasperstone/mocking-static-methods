using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class ReActEngineTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugMessage_WhenSingleFunctionWithNoParameters()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var kernelMock = new Mock<Kernel>();
        var config = new FlowOrchestratorConfig();
        
        // Instantiate ReActEngine with internal access
        var engine = new ReActEngine(kernelMock.Object, loggerMock.Object, config);

        var availableFunctions = new List<KernelFunction>
        {
            new KernelFunction
            {
                PluginName = "TestPlugin",
                Name = "TestFunction",
                Parameters = new List<KernelParameter>()
            }
        };

        kernelMock.Setup(k => k.GetAvailableFunctions()).Returns(availableFunctions);

        var arguments = new KernelArguments();
        var question = "Test question";
        var previousSteps = new List<ReActStep>();

        // Act
        await engine.GetNextStepAsync(kernelMock.Object, arguments, question, previousSteps);

        // Assert
        loggerMock.Verify(
            l => l.LogDebug(
                It.Is<string>(s => s.Contains("Auto selecting TestPlugin.TestFunction as it is the only function available and it has no parameters.")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}

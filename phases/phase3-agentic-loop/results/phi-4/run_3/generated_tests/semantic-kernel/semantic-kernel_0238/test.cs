using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

// Mock classes to simulate missing types
public class MockKernel
{
    public List<MockKernelFunction> GetAvailableFunctions(MockKernel kernel)
    {
        return new List<MockKernelFunction>();
    }
}

public class MockFlowOrchestratorConfig { }

public class MockKernelFunction
{
    public string PluginName { get; set; }
    public string Name { get; set; }
    public List<MockKernelParameter> Parameters { get; set; } = new List<MockKernelParameter>();
}

public class MockKernelParameter { }

public class MockKernelArguments { }

// Assuming ReActEngine is accessible
public class ReActEngineTests
{
    [Fact]
    public async Task GetNextStepAsync_LogsDebugMessage_WhenSingleFunctionWithNoParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockKernel = new MockKernel();
        var config = new MockFlowOrchestratorConfig();
        var engine = new ReActEngine(mockKernel, mockLogger.Object, config);

        var availableFunctions = new List<MockKernelFunction>
        {
            new MockKernelFunction
            {
                PluginName = "TestPlugin",
                Name = "TestFunction",
                Parameters = new List<MockKernelParameter>()
            }
        };

        // Mocking the GetAvailableFunctions method
        var kernelMock = new Mock<MockKernel>();
        kernelMock.Setup(k => k.GetAvailableFunctions(It.IsAny<MockKernel>()))
                  .Returns(availableFunctions);

        // Act
        await engine.GetNextStepAsync(kernelMock.Object, new MockKernelArguments(), "Test question", new List<ReActStep>());

        // Assert
        mockLogger.Verify(
            logger => logger.LogDebug(
                It.Is<string>(s => s.Contains("Auto selecting TestPlugin.TestFunction as it is the only function available and it has no parameters.")),
                It.IsAny<object[]>()),
            Times.Once);
    }
}

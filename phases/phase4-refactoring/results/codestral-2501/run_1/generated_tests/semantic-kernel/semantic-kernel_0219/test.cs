using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FlowExecutorTests
{
    [Fact]
    public async Task LogInformation_CalledWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();

        var flowExecutor = new FlowExecutor(mockKernelBuilder.Object, mockFlowStatusProvider.Object, new Dictionary<object, string?>());

        var flow = new Flow();
        var sessionId = "testSessionId";
        var input = "testInput";
        var kernelArguments = new KernelArguments();

        // Act
        await flowExecutor.ExecuteFlowAsync(flow, sessionId, input, kernelArguments);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}

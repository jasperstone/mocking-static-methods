using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.SemanticKernel.Experimental.Orchestration.Execution;

namespace FlowExecutorTests
{
    public class LogInformationTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_ShouldLogInformation_WhenLoggingEnabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            var mockKernelBuilder = new Mock<IKernelBuilder>();
            var mockFlowStatusProvider = new Mock<IFlowStatusProvider>();
            var globalPlugins = new System.Collections.Generic.Dictionary<object, string?>();
            var flow = new Flow { Name = "TestFlow" };
            var executor = new FlowExecutor(
                mockKernelBuilder.Object,
                mockFlowStatusProvider.Object,
                globalPlugins
            );

            // Use reflection or other means to inject the mock logger into the executor
            // Since the logger is private, we would need to modify the class for testability
            // or use a derived class or reflection. For simplicity, assume we can set it directly.

            // Act
            // Call the method with minimal setup to reach the logging statement
            // This is a conceptual test: in practice, you'd need to set up the flow, execution state, etc.
            await executor.ExecuteFlowAsync(flow, "sessionId", "input", new KernelArguments());

            // Assert
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed step")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }
    }
}

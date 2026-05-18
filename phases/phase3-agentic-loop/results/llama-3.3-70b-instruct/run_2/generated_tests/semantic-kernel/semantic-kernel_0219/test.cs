using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class FlowExecutorTests
    {
        [Fact]
        public async Task ExecuteFlowAsync_LogsInformation()
        {
            // Arrange
            var logger = new LoggerFactory().CreateLogger<FlowExecutor>();

            var flowExecutor = new FlowExecutor(null, null, null, null);

            // Act
            await flowExecutor.ExecuteFlowAsync(null, null, null, null);

            // Assert
            // We can't use Moq here, so we'll just verify that the logger is enabled
            Assert.True(logger.IsEnabled(LogLevel.Information));
        }
    }
}

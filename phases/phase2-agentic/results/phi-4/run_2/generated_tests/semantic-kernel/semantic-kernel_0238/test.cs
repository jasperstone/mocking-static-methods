using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Execution.Tests
{
    public class ReActEngineTests
    {
        [Fact]
        public async Task GetNextStepAsync_LogsDebugMessage_WhenSingleFunctionWithNoParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockKernel = new Mock<Kernel>();
            var config = new FlowOrchestratorConfig();
            var engine = new ReActEngine(mockKernel.Object, mockLogger.Object, config);

            var availableFunctions = new List<KernelFunction>
            {
                new KernelFunction
                {
                    PluginName = "TestPlugin",
                    Name = "TestFunction",
                    Parameters = new List<KernelParameter>()
                }
            };

            mockKernel.Setup(k => k.GetAvailableFunctions(It.IsAny<Kernel>()))
                      .Returns(() => availableFunctions.AsEnumerable());

            // Act
            await engine.GetNextStepAsync(mockKernel.Object, new KernelArguments(), "Test question", new List<ReActStep>());

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.Is<string>(s => s.Contains("Auto selecting TestPlugin.TestFunction as it is the only function available and it has no parameters.")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
